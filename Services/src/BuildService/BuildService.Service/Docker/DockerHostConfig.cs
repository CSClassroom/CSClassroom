namespace CSC.BuildService.Service.Docker
{
	/// <summary>
	/// The docker host configuration.
	/// </summary>
	public class DockerHostConfig
	{
		/// <summary>
		/// The Docker library path.
		/// </summary>
		public string DockerLibraryPath { get; }

		/// <summary>
		/// A path to the temporary folder on the host that is also
		/// mounted in the current container.
		/// </summary>
		public string HostTempFolderPath { get; }

		/// <summary>
		/// A path to the temporary folder on the client that is
		/// also mounted in this container.
		/// </summary>
		public string ContainerTempFolderPath { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public DockerHostConfig(
			string dockerLibraryPath, 
			string hostTempFolderPath, 
			string containerTempFolderPath)
		{
			DockerLibraryPath = dockerLibraryPath;
			HostTempFolderPath = hostTempFolderPath;
			ContainerTempFolderPath = containerTempFolderPath;
		}
	}
}
