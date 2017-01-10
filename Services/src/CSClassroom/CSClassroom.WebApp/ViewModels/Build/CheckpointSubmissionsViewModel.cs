using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The status of a checkpoint for a student.
	/// </summary>
	public class CheckpointSubmissionsViewModel
	{
		/// <summary>
		/// The user ID.
		/// </summary>
		public User User { get; }

		/// <summary>
		/// The submission (or lack thereof) for each checkpoint.
		/// </summary>
		public IList<CheckpointSubmissionViewModel> Checkpoints { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CheckpointSubmissionsViewModel(
			UserSubmissionResults submissions, 
			ITimeZoneProvider timeZoneProvider)
		{
			User = submissions.User;
			Checkpoints = submissions.SubmissionResults
				.Select
				(
					submission => new CheckpointSubmissionViewModel
					(
						submission,
						timeZoneProvider	
					)
				).ToList();
		}
	}
}
