using System;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The build result for the latest build.
	/// </summary>
	public class LatestBuildResult
	{
		/// <summary>
		/// The latest commit.
		/// </summary>
		public Commit Commit { get; }

		/// <summary>
		/// The latest build result, if the build is complete.
		/// </summary>
		public BuildResult BuildResult { get; }

		/// <summary>
		/// The estimated build duration, if the build is not complete.
		/// </summary>
		public TimeSpan? EstimatedBuildDuration { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public LatestBuildResult(
			Commit commit,
			BuildResult buildResult,
			TimeSpan? estimatedBuildDuration)
		{
			Commit = commit;
			BuildResult = buildResult;
			EstimatedBuildDuration = estimatedBuildDuration;
		}
	}
}
