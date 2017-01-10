using System.Threading.Tasks;

namespace CSC.BuildService.Service.Docker
{
	/// <summary>
	/// A host that allows the starting of sibling docker containers.
	/// </summary>
	public interface IDockerHostFactory
	{
		/// <summary>
		/// Pulls all container images, so the containers are ready to launch.
		/// </summary>
		Task PullContainerImagesAsync();

		/// <summary>
		/// Creates a docker host, for creating containers with the given 
		/// container config id.
		/// </summary>
		IDockerHost CreateDockerHost(string containerConfigId);
	}
}
