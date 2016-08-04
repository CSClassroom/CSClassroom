using CSC.CodeRunner.Model;
using System.Threading.Tasks;

namespace CSC.CodeRunner.Service
{
	/// <summary>
	/// The code runner service.
	/// </summary>
    public interface ICodeRunnerService
    {
		/// <summary>
		/// Executes a class job, and returns the result.
		/// </summary>
		Task<ClassJobResult> ExecuteClassJobAsync(ClassJob codeJob);

		/// <summary>
		/// Executes a method job, and returns the result.
		/// </summary>
		Task<MethodJobResult> ExecuteMethodJobAsync(MethodJob methodJob);
	}
}
