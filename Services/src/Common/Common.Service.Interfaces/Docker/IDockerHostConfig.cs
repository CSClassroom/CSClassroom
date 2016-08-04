namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// The configuration for a docker host.
	/// </summary>
    public interface IDockerHostConfig
	{
		/// <summary>
		/// The Docker library path.
		/// </summary>
		string DockerLibraryPath { get; }

		/// <summary>
		/// A path to the temporary folder on the host that is also
		/// mounted in the current container.
		/// </summary>
		string HostTempFolderPath { get; }

		/// <summary>
		/// A path to the temporary folder on the client that is
		/// also mounted in this container.
		/// </summary>
		string ContainerTempFolderPath { get; }
	}
}
