namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// The result of running a docker container.
	/// </summary>
	public class DockerResult : IDockerResult
	{
		/// <summary>
		/// Whether or not the container executed in the time alloted.
		/// </summary>
		public bool Completed { get; private set; }

		/// <summary>
		/// The response file from the docker container.
		/// </summary>
		public string Output { get; private set; }

		/// <summary>
		/// The contents of stdout and stderr for the container.
		/// </summary>
		public string Response { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public DockerResult(bool completed, string output, string response)
		{
			Completed = completed;
			Output = output;
			Response = response;
		}
	}
}
