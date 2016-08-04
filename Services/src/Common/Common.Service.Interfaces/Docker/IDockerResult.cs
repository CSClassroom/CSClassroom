namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// The result of running a docker container.
	/// </summary>
    public interface IDockerResult
    {
		/// <summary>
		/// Whether or not the container executed in the time alloted.
		/// </summary>
		bool Completed { get; }

		/// <summary>
		/// The response file from the docker container.
		/// </summary>
		string Response { get; }

		/// <summary>
		/// The contents of stdout and stderr for the container.
		/// </summary>
		string Output { get; }
    }
}
