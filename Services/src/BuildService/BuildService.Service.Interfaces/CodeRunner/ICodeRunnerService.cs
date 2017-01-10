using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;

namespace CSC.BuildService.Service.CodeRunner
{
	/// <summary>
	/// The code runner service.
	/// </summary>
	public interface ICodeRunnerService
	{
		/// <summary>
		/// Executes a method job, and returns the result.
		/// </summary>
		Task<MethodJobResult> ExecuteMethodJobAsync(MethodJob methodJob);

		/// <summary>
		/// Executes a class job, and returns the result.
		/// </summary>
		Task<ClassJobResult> ExecuteClassJobAsync(ClassJob classJob);
	}
}
