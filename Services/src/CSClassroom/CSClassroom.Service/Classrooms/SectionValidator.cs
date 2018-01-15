using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Ensures that a section is valid.
	/// </summary>
	public class SectionValidator : ISectionValidator
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionValidator(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns whether or not a section is valid.
		/// </summary>
		public async Task<bool> ValidateSectionAsync(
			Section section,
			IModelErrorCollection errors)
		{
			EnsureNoDuplicateSectionGradebooks(section, errors);
			EnsureNoDuplicateSectionRecipients(section, errors);
			await EnsureSectionRecipientsAreClassAdmins(section, errors);

			return !errors.HasErrors;
		}

		/// <summary>
		/// Ensures that there are no duplicate section gradebooks.
		/// </summary>
		private void EnsureNoDuplicateSectionGradebooks(
			Section section, 
			IModelErrorCollection errors)
		{
			if (section.SectionGradebooks != null)
			{
				var classroomGradebookIds = section.SectionGradebooks
					.Select(d => d.ClassroomGradebookId)
					.ToList();

				if (classroomGradebookIds.Distinct().Count() != classroomGradebookIds.Count)
				{
					errors.AddError
					(
						"SectionGradebooks",
						"You may only have one section gradebook per classroom gradebook."
					);
				}
			}
		}

		/// <summary>
		/// Ensures that there are no duplicate section recipients.
		/// </summary>
		private void EnsureNoDuplicateSectionRecipients(
			Section section, 
			IModelErrorCollection errors)
		{
			if (section.SectionRecipients != null)
			{
				var cmIds = section.SectionRecipients
					.Select(d => d.ClassroomMembershipId)
					.ToList();

				if (cmIds.Distinct().Count() != cmIds.Count)
				{
					errors.AddError
					(
						"SectionRecipients",
						"Duplicate section recipients are not permitted."
					);
				}
			}
		}

		/// <summary>
		/// Ensures that section recipients are class admins.
		/// </summary>
		private async Task EnsureSectionRecipientsAreClassAdmins(
			Section section, 
			IModelErrorCollection errors)
		{
			var classroomMemberships = await _dbContext.ClassroomMemberships
				.Where(cm => cm.ClassroomId == section.ClassroomId)
				.Where(cm => cm.Role >= ClassroomRole.Admin)
				.ToListAsync();

			var cmIds = classroomMemberships.Select(cm => cm.Id).ToHashSet();

			if (section.SectionRecipients != null &&
			    section.SectionRecipients.Any(sr => !cmIds.Contains(sr.ClassroomMembershipId)))
			{
				errors.AddError
				(
					"SectionRecipients", 
					"All section recipients must be class admins."
				);
			}
		}
	}
}