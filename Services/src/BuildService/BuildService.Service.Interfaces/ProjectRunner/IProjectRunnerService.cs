using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;

namespace CSC.BuildService.Service.ProjectRunner
{
	/// <summary>
	/// The project runner service.
	/// </summary>
	public interface IProjectRunnerService
	{
		/// <summary>
		/// Executes a projet job, notifying the callback path when complete.
		/// </summary>
		Task ExecuteProjectJobAsync(ProjectJob projectJob, string operationId);
	}
}
