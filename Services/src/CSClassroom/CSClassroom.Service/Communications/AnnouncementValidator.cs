using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Communications;

namespace CSC.CSClassroom.Service.Communications
{
	/// <summary>
	/// Ensures that a new or existing announcement is valid.
	/// </summary>
	public class AnnouncementValidator : IAnnouncementValidator
	{
		/// <summary>
		/// Validates that an announcement is correctly configured.
		/// </summary>
		public bool ValidateAnnouncement(
			Classroom classroom,
			Announcement announcement,
			IModelErrorCollection modelErrors)
		{
			var sectionIds = announcement.Sections
				?.Select(s => s.SectionId)
				?.ToList() ?? new List<int>();

			if (!sectionIds.Any())
			{
				modelErrors.AddError
				(
					"Sections",
					"At least one section must be included."
				);

				return false;
			}

			if (sectionIds.Distinct().Count() != sectionIds.Count)
			{
				modelErrors.AddError
				(
					"Sections",
					"Duplicate sections are not permitted."
				);

				return false;
			}

			if (sectionIds
				.Intersect(classroom.Sections.Select(s => s.Id))
				.Count() != sectionIds.Count)
			{
				modelErrors.AddError
				(
					"Sections",
					"Invalid sections selected."
				);

				return false;
			}

			return true;
		}
	}
}