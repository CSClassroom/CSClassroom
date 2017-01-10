using System.Collections.Generic;

namespace CSC.BuildService.Model.ProjectRunner
{
	/// <summary>
	/// Builds and tests a project.
	/// </summary>
	public class ProjectJob
	{
		/// <summary>
		/// The token used to request the build.
		/// </summary>
		public string BuildRequestToken { get; }

		/// <summary>
		/// The GitHub organization containing the repository.
		/// </summary>
		public string GitHubOrg { get; }

		/// <summary>
		/// The name of the project.
		/// </summary>
		public string ProjectName { get; }

		/// <summary>
		/// The name of the submission repository.
		/// </summary>
		public string SubmissionRepo { get; }

		/// <summary>
		/// The name of the project template repository.
		/// </summary>
		public string TemplateRepo { get; }

		/// <summary>
		/// The SHA of the commit to build.
		/// </summary>
		public string CommitSha { get; }

		/// <summary>
		/// The list of paths to copy from the template repository
		/// to the submission repository, before building and running tests.
		/// </summary>
		public IList<string> CopyPaths { get; }

		/// <summary>
		/// The list of test classes to run.
		/// </summary>
		public IList<string> TestClasses { get; }

		/// <summary>
		/// The path of the URL to call back with the results.
		/// </summary>
		public string CallbackPath { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectJob(
			string buildRequestToken,
			string gitHubOrg,
			string projectName,
			string submissionRepo,
			string templateRepo,
			string commitSha,
			IList<string> copyPaths,
			IList<string> testClasses,
			string callbackPath)
		{
			BuildRequestToken = buildRequestToken;
			GitHubOrg = gitHubOrg;
			ProjectName = projectName;
			SubmissionRepo = submissionRepo;
			TemplateRepo = templateRepo;
			CommitSha = commitSha;
			CopyPaths = copyPaths;
			TestClasses = testClasses;
			CallbackPath = callbackPath;
		}
	}
}
