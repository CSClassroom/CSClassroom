using System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.WebApp.ViewModels.Build;

namespace CSC.CSClassroom.WebApp.ViewModels.Project
{
	/// <summary>
	/// The view model for a project's status.
	/// </summary>
	public class ProjectStatusViewModel
	{
		/// <summary>
		/// The project name.
		/// </summary>
		public string ProjectName { get; }

		/// <summary>
		/// The last commit date.
		/// </summary>
		public DateTime LastCommitDate { get; }

		/// <summary>
		/// Whether or not the last build succeeded.
		/// </summary>
		public bool LastBuildSucceeded { get; }

		/// <summary>
		/// Test counts for the build, in ascending order.
		/// </summary>
		public TestTrendViewModel TestTrend { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectStatusViewModel(
			ProjectStatus projectStatus)
		{
			ProjectName = projectStatus.ProjectName;
			LastCommitDate = projectStatus.LastCommitDate;
			LastBuildSucceeded = projectStatus.LastBuildSucceeded;
			TestTrend = new TestTrendViewModel
			(
				projectStatus.BuildTestCounts,
				projectStatus.ProjectName,
				currentBuild: null,
				thumbnail: true
			);
		}
	}
}
