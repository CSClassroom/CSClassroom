namespace CSC.BuildService.Service.ProjectRunner
{
	/// <summary>
	/// The configuration of the project runner service.
	/// </summary>
	public class ProjectRunnerServiceConfig : IProjectRunnerServiceConfig
	{
		/// <summary>
		/// The OAuth token for GitHub access.
		/// </summary>
		public string GitHubOAuthToken { get; }

		/// <summary>
		/// The host that receives the results of project jobs.
		/// </summary>
		public string ProjectJobResultHost { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectRunnerServiceConfig(string gitHubOAuthToken, string projectJobResultHost)
		{
			GitHubOAuthToken = gitHubOAuthToken;
			ProjectJobResultHost = projectJobResultHost;
		}
	}
}
