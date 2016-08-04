using System;

namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// The configuration for a docker container.
	/// </summary>
	public interface IDockerContainerConfig
    {
		/// <summary>
		/// The name of the image to run in the container.
		/// </summary>
		string ImageName { get; }

		/// <summary>
		/// The mount point in the newly created container for the folder
		/// that will contain the request and response files.
		/// </summary>
		string RequestResponseMountPoint { get; }

		/// <summary>
		/// The name of the request file that a container will read.
		/// </summary>
		string RequestFileName { get; }

		/// <summary>
		/// The name of the response file that a container will write.
		/// </summary>
		string ResponseFileName { get; }

		/// <summary>
		///  The maximum lifetime of the container. If a container takes
		///  longer than this lifetime, it will be killed.
		/// </summary>
		TimeSpan MaxLifetime { get; }
    }
}
