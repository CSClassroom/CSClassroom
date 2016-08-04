using Microsoft.Extensions.Configuration;
using System;

namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// A docker container configuration.
	/// </summary>
    public class DockerContainerConfig : IDockerContainerConfig
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		public DockerContainerConfig(IConfigurationSection dockerContainerSettings)
		{
			ImageName = dockerContainerSettings["ImageName"];
			RequestResponseMountPoint = dockerContainerSettings["RequestResponseMountPoint"];
			RequestFileName = dockerContainerSettings["RequestFileName"];
			ResponseFileName = dockerContainerSettings["ResponseFileName"];
			MaxLifetime = TimeSpan.Parse(dockerContainerSettings["MaxLifetime"]);
		}

		/// <summary>
		/// The name of the image to run in the container.
		/// </summary>
		public string ImageName { get; }

		/// <summary>
		/// The mount point in the newly created container for the folder
		/// that will contain the request and response files.
		/// </summary>
		public string RequestResponseMountPoint { get; }

		/// <summary>
		/// The name of the request file that a container will read.
		/// </summary>
		public string RequestFileName { get; }

		/// <summary>
		/// The name of the response file that a container will write.
		/// </summary>
		public string ResponseFileName { get; }

		/// <summary>
		///  The maximum lifetime of the container. If a container takes
		///  longer than this lifetime, it will be killed.
		/// </summary>
		public TimeSpan MaxLifetime { get; }
	}
}
