using System;
using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The view model for a build in progress.
	/// </summary>
	public class BuildInProgressViewModel
	{
		/// <summary>
		/// The commit for the build in progress.
		/// </summary>
		public Commit Commit { get; }

		/// <summary>
		/// The estimated duration of the build.
		/// </summary>
		public TimeSpan EstimatedDuration { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BuildInProgressViewModel(Commit commit, TimeSpan estimatedDuration)
		{
			Commit = commit;
			EstimatedDuration = estimatedDuration;
		}
	}
}
