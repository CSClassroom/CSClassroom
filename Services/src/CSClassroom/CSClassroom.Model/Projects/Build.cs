using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// Build results for a single commit of a project by a user.
	/// </summary>
	public class Build
	{
		/// <summary>
		/// The ID of the build.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The commit for which this build belongs to.
		/// </summary>
		public int CommitId { get; set; }

		/// <summary>
		/// The commit for which this build belongs to.
		/// </summary>
		public virtual Commit Commit { get; set; }

		/// <summary>
		/// The date that the build started.
		/// </summary>
		public DateTime DateStarted { get; set; }

		/// <summary>
		/// The date that the build completed.
		/// </summary>
		public DateTime DateCompleted { get; set; }

		/// <summary>
		/// The status of the build.
		/// </summary>
		public BuildStatus Status { get; set; }

		/// <summary>
		/// The output of the build.
		/// </summary>
		public string Output { get; set; }

		/// <summary>
		/// The test results for the build.
		/// </summary>
		public IList<TestResult> TestResults { get; set; }
	}
}
