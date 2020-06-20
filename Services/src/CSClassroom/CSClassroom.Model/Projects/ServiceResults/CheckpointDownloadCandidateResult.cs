using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// Information about one section's project checkpoint, offered to the SubmissionController
	/// as a candidate for downloading
	/// </summary>
	public class CheckpointDownloadCandidateResult
	{
		/// <summary>
		/// The section for this project checkpoint
		/// </summary>
		public Section Section { get; }

		/// <summary>
		/// List of users in this section for the checkpoint
		/// </summary>
		public IList<UserDownloadCandidateResult> Users { get; }

		public CheckpointDownloadCandidateResult(Section section, IList<UserDownloadCandidateResult> users)
		{
			Section = section;
			Users = users;
		}
	}

	/// <summary>
	/// Information about one student's project checkpoint, offered to the SubmissionController
	/// as a candidate for downloading
	/// </summary>
	public class UserDownloadCandidateResult
	{
		/// <summary>
		/// The user whose commit for this checkpoint is offered as an option fwwor downloading 
		/// </summary>
		public User User { get; }

		/// <summary>
		/// Did the student actually turn in a commit for this checkpoint?
		/// </summary>
		public bool Submitted { get;  }

		public UserDownloadCandidateResult(User user, bool submitted)
		{
			User = user;
			Submitted = submitted;
		}
	}
}
