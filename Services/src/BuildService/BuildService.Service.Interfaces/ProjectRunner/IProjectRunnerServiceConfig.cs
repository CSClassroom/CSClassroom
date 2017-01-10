namespace CSC.BuildService.Service.ProjectRunner
{
	/// <summary>
	/// The configuration of the project runner service.
	/// </summary>
	public interface IProjectRunnerServiceConfig
	{
		/// <summary>
		/// The OAuth token for GitHub access.
		/// </summary>
		string GitHubOAuthToken { get; }

		/// <summary>
		/// The host that receives the results of project jobs.
		/// </summary>
		string ProjectJobResultHost { get; }
	}
}
