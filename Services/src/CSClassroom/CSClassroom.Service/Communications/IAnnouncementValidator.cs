using System;
using System.Collections.Generic;
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
	public interface IAnnouncementValidator
	{
		/// <summary>
		/// Validates that an announcement is correctly configured.
		/// </summary>
		bool ValidateAnnouncement(
			Classroom classroom,
			Announcement announcement,
			IModelErrorCollection modelErrors);
	}
}