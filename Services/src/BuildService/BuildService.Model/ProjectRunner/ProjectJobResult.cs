using System;
using System.Collections.Generic;

namespace CSC.BuildService.Model.ProjectRunner
{
	/// <summary>
	/// The result of a project job.
	/// </summary>
	public class ProjectJobResult
	{
		/// <summary>
		/// The token used to request the build.
		/// </summary>
		public string BuildRequestToken { get; set; }

		/// <summary>
		/// The status of the project job.
		/// </summary>
		public ProjectJobStatus Status { get; set; }

		/// <summary>
		/// The date that the job started.
		/// </summary>
		public DateTime JobStartedDate { get; set; }

		/// <summary>
		/// The date that the job finished.
		/// </summary>
		public DateTime JobFinishedDate { get; set; }

		/// <summary>
		/// The output of the build process.
		/// </summary>
		public string BuildOutput { get; set; }

		/// <summary>
		/// A list of test results.
		/// </summary>
		public List<TestResult> TestResults { get; set; }

		/// <summary>
		/// Whether or not the build succeeded.
		/// </summary>
		public bool BuildSucceeded => TestResults != null;
	}
}
