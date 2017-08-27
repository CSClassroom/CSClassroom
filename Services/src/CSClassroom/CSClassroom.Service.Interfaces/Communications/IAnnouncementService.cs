using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Communications;

namespace CSC.CSClassroom.Service.Communications
{
	/// <summary>
	/// The announcement service.
	/// </summary>
	public interface IAnnouncementService
	{
		/// <summary>
		/// Gets a list of announcements for the given class that are visible
		/// to the given user.
		/// </summary>
		Task<IOrderedQueryable<Announcement>> GetAnnouncementsAsync(
			string classroomName, 
			int userId,
			bool admin);

		/// <summary>
		/// Returns an announcement with the given ID.
		/// </summary>
		Task<Announcement> GetAnnouncementAsync(
			string classroomName,
			int announcementId);

		/// <summary>
		/// Publish a new announcement.
		/// </summary>
		Task<bool> PostAnnouncementAsync(
			string classroomName,
			int userId,
			Announcement announcement,
			Func<DateTime, string> formatDateTime,
			IModelErrorCollection modelErrors);

		/// <summary>
		/// Edits an announcement.
		/// </summary>
		Task<bool> EditAnnouncementAsync(
			string classroomName,
			Announcement announcement,
			Func<DateTime, string> formatDateTime,
			IModelErrorCollection modelErrors);

		/// <summary>
		/// Deletes an announcement.
		/// </summary>
		Task DeleteAnnouncementAsync(
			string classroomName, 
			int announcementId);
	}
}
