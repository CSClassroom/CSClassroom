using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The view model for a build.
	/// </summary>
	public class BuildViewModel
	{
		/// <summary>
		/// The user who committed the code.
		/// </summary>
		public User User { get; }

		/// <summary>
		/// Submission information for each checkpoint.
		/// </summary>
		public CheckpointSubmissionsViewModel Submissions { get; }

		/// <summary>
		/// The push date for this build.
		/// </summary>
		public DateTime PushDate { get; }

		/// <summary>
		/// The commit message.
		/// </summary>
		public string CommitMessage { get; }

		/// <summary>
		/// The SHA of the commit.
		/// </summary>
		public string CommitSha { get; }

		/// <summary>
		/// The URL for the commit.
		/// </summary>
		public string CommitUrl { get; }

		/// <summary>
		/// The status of the build.
		/// </summary>
		public BuildStatus BuildStatus { get; }

		/// <summary>
		/// The duration of the build.
		/// </summary>
		public TimeSpan BuildDuration { get; }

		/// <summary>
		/// The build output.
		/// </summary>
		public string BuildOutput { get; }

		/// <summary>
		/// The test results for this build.
		/// </summary>
		public IList<TestClassTableEntry> TestClassResults { get; }

		/// <summary>
		/// Whether or not this build is the latest build.
		/// </summary>
		public bool IsLatestBuild { get; }

		/// <summary>
		/// The passed/failed tests counts for all builds.
		/// </summary>
		public TestTrendViewModel TestTrend { get; }

		/// <summary>
		/// Unread feedback.
		/// </summary>
		public IList<UnreadFeedbackViewModel> UnreadFeedback { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BuildViewModel(
			BuildResult buildResult,
			IList<UnreadFeedbackResult> unreadFeedback,
			Func<Model.Projects.Build, string> commitUrlBuilder,
			Func<TestResult, string> testUrlBuilder,
			Func<string, string, int, string> feedbackUrlBuilder,
			ITimeZoneProvider timeZoneProvider)
		{
			User = buildResult.Build.Commit.User;
			Submissions = new CheckpointSubmissionsViewModel
			(
				buildResult.Submissions, 
				timeZoneProvider
			);
			PushDate = buildResult.Build.Commit.PushDate;
			CommitMessage = buildResult.Build.Commit.Message;
			CommitSha = buildResult.Build.Commit.Sha;
			CommitUrl = commitUrlBuilder(buildResult.Build);
			BuildStatus = buildResult.Build.Status;
			BuildDuration = buildResult.Build.DateCompleted - buildResult.Build.DateStarted;
			BuildOutput = buildResult.Build.Output;
			TestClassResults = TestClassTableEntry.GetTestClassResults
			(
				null /*checkpoint*/,
				buildResult.Build, 
				testUrlBuilder
			);
			IsLatestBuild = buildResult.LatestBuild;
			TestTrend = buildResult.Build.Status == BuildStatus.Completed
				? new TestTrendViewModel
					(
						buildResult.AllBuildTestCounts, 
						buildResult.Build.Commit.Project.Name, 
						buildResult.Build, 
						thumbnail: false
					)
				: null;
			UnreadFeedback = unreadFeedback.Select
			(
				uf => new UnreadFeedbackViewModel
				(
					uf, 
					feedbackUrlBuilder, 
					timeZoneProvider
				)
			).ToList();
		}
	}
}
