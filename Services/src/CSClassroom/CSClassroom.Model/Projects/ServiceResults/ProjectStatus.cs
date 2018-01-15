using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The status of a project's build.
	/// </summary>
	public class ProjectStatus
	{
		/// <summary>
		/// The project name.
		/// </summary>
		public string ProjectName { get; }

		/// <summary>
		/// The repository name.
		/// </summary>
		public string RepoName { get; }

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
		public IList<BuildTestCount> BuildTestCounts { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectStatus(
			string projectName,
			string repoName,
			DateTime lastCommitDate,
			bool lastBuildSucceeded,
			IList<BuildTestCount> buildTestCounts)
		{
			ProjectName = projectName;
			RepoName = repoName;
			LastCommitDate = lastCommitDate;
			LastBuildSucceeded = lastBuildSucceeded;
			BuildTestCounts = buildTestCounts;
		}
	}
}
