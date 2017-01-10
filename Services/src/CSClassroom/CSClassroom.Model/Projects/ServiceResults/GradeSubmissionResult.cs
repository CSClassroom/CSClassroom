using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// A submission result for grading a student's submission.
	/// </summary>
	public class GradeSubmissionResult
	{
		/// <summary>
		/// The student's last name.
		/// </summary>
		public string LastName { get; }

		/// <summary>
		/// The student's first name.
		/// </summary>
		public string FirstName { get; }

		/// <summary>
		/// The submission id.
		/// </summary>
		public int SubmissionId { get; }

		/// <summary>
		/// The commit date.
		/// </summary>
		public DateTime CommitDate { get; }

		/// <summary>
		/// The number of days the submission is late.
		/// </summary>
		public int DaysLate { get; }

		/// <summary>
		/// The pull request number.
		/// </summary>
		public int? PullRequestNumber { get; }

		/// <summary>
		/// Whether or not all required tests passed.
		/// </summary>
		public bool RequiredTestsPassed { get; }

		/// <summary>
		/// The feedback for the submission.
		/// </summary>
		public string Feedback { get; }

		/// <summary>
		/// Whether or not the feedback was sent.
		/// </summary>
		public bool FeedbackSent { get; }

		/// <summary>
		/// The build.
		/// </summary>
		public Build Build { get; }

		/// <summary>
		/// Past submissions.
		/// </summary>
		public IList<PastSubmissionResult> PastSubmissions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GradeSubmissionResult(
			User user, 
			Section section,
			Submission currentSubmission,
			IList<Submission> pastSubmissions)
		{
			LastName = user.LastName;
			FirstName = user.FirstName;
			SubmissionId = currentSubmission.Id;
			CommitDate = currentSubmission.Commit.PushDate;
			DaysLate = currentSubmission.GetDaysLate(section);
			PullRequestNumber = currentSubmission.PullRequestNumber;
			Feedback = currentSubmission.Feedback;
			FeedbackSent = currentSubmission.FeedbackSent;
			Build = currentSubmission.Commit.Build;
			PastSubmissions = pastSubmissions
				.Select(ps => new PastSubmissionResult(ps, section))
				.ToList();

			RequiredTestsPassed = 
				currentSubmission.Commit.Build.Status == BuildStatus.Completed &&
				currentSubmission.Commit.Build.TestResults
					.Select
					(
						tr => new
						{
							Required = currentSubmission.Checkpoint
								.TestClasses
								.FirstOrDefault
								(
									tc => tc.TestClass.ClassName == tr.ClassName
								)?.Required ?? false,
							Passed = tr.Succeeded
						}
					)
					.All
					(
						tr => !tr.Required || tr.Passed
					);
		}
	}
}
