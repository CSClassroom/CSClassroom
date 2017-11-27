using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.Security;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Communications
{
	/// <summary>
	/// The announcement service.
	/// </summary>
	public class AnnouncementService : IAnnouncementService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The announcement validator.
		/// </summary>
		private readonly IAnnouncementValidator _validator;

		/// <summary>
		/// An HTML sanitizer.
		/// </summary>
		private readonly IHtmlSanitizer _htmlSanitizer;

		/// <summary>
		/// An e-mail provider.
		/// </summary>
		private readonly IEmailProvider _emailProvider;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AnnouncementService(
			DatabaseContext dbContext,
			IAnnouncementValidator validator,
			IHtmlSanitizer htmlSanitizer,
			IEmailProvider emailProvider,
			ITimeProvider timeProvider)
		{
			_dbContext = dbContext;
			_validator = validator;
			_htmlSanitizer = htmlSanitizer;
			_emailProvider = emailProvider;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Gets a list of announcements for the given class that are visible
		/// to the given user.
		/// </summary>
		public async Task<IOrderedQueryable<Announcement>> GetAnnouncementsAsync(
			string classroomName,
			int userId,
			bool admin)
		{
			var announcementsQuery = _dbContext.Announcements
				.Include(a => a.User)
				.Where(a => a.Classroom.Name == classroomName);

			if (admin)
			{
				announcementsQuery = announcementsQuery
					.Include(a => a.Sections)
					.ThenInclude(s => s.Section);
			}
			else
			{
				var classroomMembership = await _dbContext.ClassroomMemberships
					.Where(cm => cm.Classroom.Name == classroomName)
					.Where(cm => cm.UserId == userId)
					.Include(cm => cm.SectionMemberships)
					.SingleOrDefaultAsync();

				var studentSectionIds = classroomMembership
					.SectionMemberships
					.Select(sm => sm.SectionId);

				announcementsQuery = announcementsQuery
					.Where(a => a.Sections.Any(s => studentSectionIds.Contains(s.SectionId)));
			}

			return announcementsQuery.OrderByDescending(a => a.DatePosted);
		}

		/// <summary>
		/// Returns an announcement with the given ID.
		/// </summary>
		public async Task<Announcement> GetAnnouncementAsync(
			string classroomName,
			int announcementId)
		{
			return await _dbContext.Announcements
				.Where(a => a.Classroom.Name == classroomName)
				.Include(a => a.Sections)
				.ThenInclude(s => s.Section)
				.Include(a => a.User)
				.SingleOrDefaultAsync(a => a.Id == announcementId);
		}

		/// <summary>
		/// Publish a new announcement.
		/// </summary>
		public async Task<bool> PostAnnouncementAsync(
			string classroomName, 
			int userId,
			Announcement announcement,
			Func<DateTime, string> formatDateTime,
			IModelErrorCollection modelErrors)
		{
			var classroom = await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.Include(c => c.Sections)
				.SingleAsync();

			var sectionIds = announcement.Sections
				?.Select(s => s.SectionId)
				?.ToList() ?? new List<int>();

			if (!_validator.ValidateAnnouncement(classroom, announcement, modelErrors))
			{
				return false;
			}

			announcement.ClassroomId = classroom.Id;
			announcement.UserId = userId;
			announcement.Contents = _htmlSanitizer.SanitizeHtml(announcement.Contents);
			announcement.DatePosted = _timeProvider.UtcNow;

			_dbContext.Announcements.Add(announcement);

			await _dbContext.SaveChangesAsync();

			await SendAnnouncementEmailAsync
			(
				announcement,
				classroom,
				sectionIds,
				formatDateTime,
				emailAdmins: true
			);

			return true;
		}

		/// <summary>
		/// Publish a new announcement.
		/// </summary>
		public async Task<bool> EditAnnouncementAsync(
			string classroomName,
			Announcement announcement,
			Func<DateTime, string> formatDateTime,
			IModelErrorCollection modelErrors)
		{
			var existingAnnouncement = await _dbContext.Announcements
				.AsNoTracking()
				.Where(a => a.Classroom.Name == classroomName)
				.Include(a => a.Sections)
				.Include(a => a.Classroom.Sections)
				.SingleOrDefaultAsync(a => a.Id == announcement.Id);

			var classroom = existingAnnouncement.Classroom;

			if (!_validator.ValidateAnnouncement(classroom, announcement, modelErrors))
			{
				return false;
			}

			announcement.ClassroomId = existingAnnouncement.ClassroomId;
			announcement.UserId = existingAnnouncement.UserId;
			announcement.DatePosted = existingAnnouncement.DatePosted;
			announcement.Contents = _htmlSanitizer.SanitizeHtml(announcement.Contents);
			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.AnnouncementSections,
				announcementSection => announcementSection.Id,
				announcementSection => announcementSection.AnnouncementId == announcement.Id,
				announcement.Sections
			);

			var newSectionIds = announcement.Sections
				.Select(s => s.SectionId)
				.Where
				(
					sectionId => !existingAnnouncement.Sections.Any
					(
						s => s.SectionId == sectionId
					)
				).ToList();

			_dbContext.Entry(existingAnnouncement).State = EntityState.Detached;
			_dbContext.Announcements.Update(announcement);

			await _dbContext.SaveChangesAsync();

			await SendAnnouncementEmailAsync
			(
				announcement,
				classroom,
				newSectionIds,
				formatDateTime,
				emailAdmins: false
			);

			return true;
		}

		/// <summary>
		/// Deletes an announcement.
		/// </summary>
		public async Task DeleteAnnouncementAsync(
			string classroomName,
			int announcementId)
		{
			var announcement = await _dbContext.Announcements.FindAsync(announcementId);
			_dbContext.Announcements.Remove(announcement);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Sends an e-mail with the announcement to all students in the section,
		/// and all admins of the classroom.
		/// </summary>
		private async Task SendAnnouncementEmailAsync(
			Announcement announcement, 
			Classroom classroom, 
			IList<int> sectionIds,
			Func<DateTime, string> formatDateTime,
			bool emailAdmins)
		{
			var students = await _dbContext.SectionMemberships
				.Where(sm => sm.ClassroomMembership.ClassroomId == classroom.Id)
				.Where(sm => sm.Role == SectionRole.Student)
				.Where(sm => sectionIds.Contains(sm.SectionId))
				.Include(sm => sm.ClassroomMembership.User)
				.Include(sm => sm.ClassroomMembership.User.AdditionalContacts)
				.ToListAsync();

			var users = students.Select(sm => sm.ClassroomMembership.User).ToList();

			if (emailAdmins)
			{
				var admins = await _dbContext.ClassroomMemberships
					.Where(cm => cm.ClassroomId == classroom.Id)
					.Where(cm => cm.Role == ClassroomRole.Admin)
					.Include(sm => sm.User)
					.Include(sm => sm.User.AdditionalContacts)
					.ToListAsync();

				users.AddRange(admins.Select(a => a.User));
			}

			if (users.Any())
			{
				if (announcement.User == null)
				{
					await _dbContext.Entry(announcement).Reference(a => a.User).LoadAsync();
				}

				var recipients = new List<EmailRecipient>();
				foreach (var user in users.Distinct())
				{
					recipients.Add
					(
						new EmailRecipient
						(
							$"{user.FirstName} {user.LastName}",
							user.EmailAddress
						)
					);

					if (user.AdditionalContacts != null)
					{
						foreach (var additionalContact in user.AdditionalContacts)
						{
							recipients.Add
							(
								new EmailRecipient
								(
									$"{additionalContact.FirstName} {additionalContact.LastName}",
									additionalContact.EmailAddress
								)
							);
						}
					}
				}
				
				await _emailProvider.SendMessageAsync
				(
					recipients,
					$"{classroom.DisplayName}: {announcement.Title}",
					announcement.Contents + GetEmailFooter(announcement, formatDateTime)
				);
			}
		}

		/// <summary>
		/// Returns the footer appended to all announcement e-mails.
		/// </summary>
		private string GetEmailFooter(
			Announcement announcement, 
			Func<DateTime, string> formatDateTime)
		{
			return _htmlSanitizer.SanitizeHtml
			(
				"<hr style=\"line-height: 5px; margin-bottom: 2px; margin-top: 10px\"/>"
				+ "<span style=\"font-size: x-small; font-weight: bold\">"
				+ $"Posted by {announcement.User.PubliclyDisplayedName} "
				+ $"on {formatDateTime(announcement.DatePosted)}</span>"
			);
		}
	}
}
