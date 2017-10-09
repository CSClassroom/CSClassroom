using System;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// A submission result for a student in a section.
	/// </summary>
	public class SectionSubmissionResult
	{
		/// <summary>
		/// The student's last name.
		/// </summary>
		[Display(Name = "Last Name")]
		public string LastName { get; }

		/// <summary>
		/// The student's first name.
		/// </summary>
		[Display(Name = "First Name")]
		public string FirstName { get; }

		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// The commit date.
		/// </summary>
		[Display(Name = "Commit Date")]
		public DateTime? CommitDate { get; }

		/// <summary>
		/// The submission date.
		/// </summary>
		[Display(Name = "Submission Date")]
		public DateTime? SubmissionDate { get; }

		/// <summary>
		/// The pull request number.
		/// </summary>
		[Display(Name = "Pull Request")]
		public int? PullRequestNumber { get; }

		/// <summary>
		/// The build ID.
		/// </summary>
		[Display(Name = "Build")]
		public int? BuildId { get; }

		/// <summary>
		/// The commit that was submitted.
		/// </summary>
		public Commit Commit { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionSubmissionResult(
			User user,
			Submission submission)
		{
			LastName = user.LastName;
			FirstName = user.FirstName;
			UserId = user.Id;
			CommitDate = submission?.Commit?.PushDate;
			SubmissionDate = submission?.DateSubmitted;
			PullRequestNumber = submission?.PullRequestNumber;
			BuildId = submission?.Commit?.Build?.Id;
			Commit = submission?.Commit;
		}
	}
}
