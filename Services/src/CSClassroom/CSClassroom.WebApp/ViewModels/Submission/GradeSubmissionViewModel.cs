using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewModels.Build;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// A view model for grading a submission.
	/// </summary>
	public class GradeSubmissionViewModel : SubmissionTableEntry
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
		/// The submission ID.
		/// </summary>
		public int SubmissionId { get; }

		/// <summary>
		/// The commit date and build link/status.
		/// </summary>
		public string Committed { get; }

		/// <summary>
		/// The submission date and status.
		/// </summary>
		public string Submitted { get; }

		/// <summary>
		/// The pull request link.
		/// </summary>
		public string PullRequest { get; }

		/// <summary>
		/// Whether or not the requried tests passed.
		/// </summary>
		public string RequiredTests { get; }

		/// <summary>
		/// The feedback for the submission.
		/// </summary>
		public string Feedback { get; }

		/// <summary>
		/// Whether or not the feedback was sent.
		/// </summary>
		public bool FeedbackSent { get; }

		/// <summary>
		/// The results of all tests in the build.
		/// </summary>
		public IList<TestClassTableEntry> TestClassResults { get; }

		/// <summary>
		/// Past submissions.
		/// </summary>
		public IList<PastSubmissionTableEntry> PastSubmissions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GradeSubmissionViewModel(
			Checkpoint checkpoint,
			GradeSubmissionResult result, 
			Func<TestResult, string> testUrlBuilder,
			Func<int, string> buildUrlBuilder,
			Func<Commit, int, string> pullRequestUrlBuilder,
			ITimeZoneProvider timeZoneProvider)
		{
			LastName = result.LastName;
			FirstName = result.FirstName;
			SubmissionId = result.SubmissionId;

			Committed = GetStatus
			(
				result.CommitDate, 
				result.CommitDaysLate, 
				buildUrlBuilder(result.Build.Id),
				timeZoneProvider
			);

			Submitted = GetStatus
			(
				result.SubmissionDate, 
				result.SubmissionDaysLate, 
				null /*buildUrl*/, 
				timeZoneProvider
			);

			PullRequest = GetLink
			(
				pullRequestUrlBuilder(result.Build.Commit, result.PullRequestNumber.Value),
				"See Submission",
				preventWrapping: true
			);

			RequiredTests = result.RequiredTestsPassed
				? GetColoredText("green", "Passed", bold: false, preventWrapping: true)
				: GetColoredText("red", "Failed", bold: true, preventWrapping: true);

			Feedback = result.Feedback;
			FeedbackSent = result.FeedbackSent;

			TestClassResults = TestClassTableEntry.GetTestClassResults
			(
				checkpoint,
				result.Build,
				testUrlBuilder
			);

			PastSubmissions = result.PastSubmissions.Select
			(
				pastSubmission => new PastSubmissionTableEntry
				(
					pastSubmission,
					buildUrlBuilder,
					pullRequestUrlBuilder,
					timeZoneProvider
				)
			).ToList();
		}
	}
}
