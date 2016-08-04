using Microsoft.Extensions.Configuration;

namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// The docker host configuration.
	/// </summary>
    public class DockerHostConfig : IDockerHostConfig
    {
		/// <summary>
		/// The docker host configuration section.
		/// </summary>
		private IConfigurationSection _dockerHostSettings;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DockerHostConfig(IConfigurationSection dockerHostSettings)
		{
			_dockerHostSettings = dockerHostSettings;
		}

		/// <summary>
		/// The Docker library path.
		/// </summary>
		public string DockerLibraryPath => _dockerHostSettings["DockerLibraryPath"];

		/// <summary>
		/// A path to the temporary folder on the host that is also
		/// mounted in the current container.
		/// </summary>
		public string HostTempFolderPath => _dockerHostSettings["HostTempFolderPath"];

		/// <summary>
		/// A path to the temporary folder on the client that is
		/// also mounted in this container.
		/// </summary>
		public string ContainerTempFolderPath => _dockerHostSettings["ContainerTempFolderPath"];
	}
}
