using System;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// Counts of passed and failed tests for a given build.
	/// </summary>
	public class BuildTestCount
	{
		/// <summary>
		/// The build link.
		/// </summary>
		public int BuildId { get; }

		/// <summary>
		/// The date of the push that triggered the build.
		/// </summary>
		public DateTime PushDate { get; }

		/// <summary>
		/// The number of tests that passed.
		/// </summary>
		public int PassedCount { get; }

		/// <summary>
		/// The number of tests that failed.
		/// </summary>
		public int FailedCount { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BuildTestCount(int buildId, DateTime pushDate, int passedCount, int failedCount)
		{
			BuildId = buildId;
			PushDate = pushDate;
			PassedCount = passedCount;
			FailedCount = failedCount;
		}
	}
}
