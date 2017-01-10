using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The result of a build returned by the Build service.
	/// </summary>
	public class BuildResult
	{
		/// <summary>
		/// The build.
		/// </summary>
		public Build Build { get; }

		/// <summary>
		/// Whether or not this is the latest build.
		/// </summary>
		public bool LatestBuild { get; }

		/// <summary>
		/// Submission information for each checkpoint.
		/// </summary>
		public UserSubmissionResults Submissions { get; }

		/// <summary>
		/// All passed/failed test counts for all builds.
		/// </summary>
		public IList<BuildTestCount> AllBuildTestCounts { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BuildResult(
			Build build,
			bool latestBuild,
			Section section,
			IList<Checkpoint> checkpoints,
			IList<Submission> submissions,
			IList<BuildTestCount> allBuildTestCounts)
		{
			Build = build;

			LatestBuild = latestBuild;

			Submissions = new UserSubmissionResults
			(
				build.Commit.User,
				section,
				checkpoints,
				submissions
			);

			AllBuildTestCounts = allBuildTestCounts;
		}
	}
}
