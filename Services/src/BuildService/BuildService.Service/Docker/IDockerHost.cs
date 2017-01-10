using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.BuildService.Service.Docker
{
	/// <summary>
	/// A host that allows the starting of sibling docker containers.
	/// </summary>
	public interface IDockerHost
	{
		/// <summary>
		/// Runs a container. The request is written to a folder mounted in
		/// the container, at a location determined by the configuration. 
		/// The container is expected to write the response file prior to 
		/// completion. This method returns a task that completes when the 
		/// container finishes running.
		/// </summary>
		/// <param name="requestContents">The request contents.</param>
		/// <param name="environmentVariables">The environment variables.</param>
		/// <returns>The response file (if any), and the contents of stdout/stderr.</returns>
		Task<DockerResult> RunImageInNewContainerAsync(
			string requestContents, 
			IDictionary<string, string> environmentVariables);
	}
}
