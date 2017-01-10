using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;

namespace CSC.BuildService.Service.ProjectRunner
{
	/// <summary>
	/// Notifies the source of the project job that the job is complete.
	/// </summary>
	public interface IProjectJobResultNotifier
	{
		/// <summary>
		/// Notifies the source of the project job that the job is complete.
		/// </summary>
		/// <param name="callbackHost">The callback host.</param>
		/// <param name="callbackPath">The callback path.</param>
		/// <param name="operationId">The operation ID (for logging purposes).</param>
		/// <param name="result">The result to post.</param>
		Task NotifyAsync(
			string callbackHost, 
			string callbackPath, 
			string operationId, 
			ProjectJobResult result);
	}
}
