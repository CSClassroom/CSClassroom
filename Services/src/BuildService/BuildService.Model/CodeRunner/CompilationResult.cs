using System.Collections.Generic;

namespace CSC.BuildService.Model.CodeRunner
{
	/// <summary>
	/// The result of a compilation.
	/// </summary>
	public class CompilationResult
	{
		/// <summary>
		/// Whether or not the compilation succeeded.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The compile errors, if the compilation failed.
		/// </summary>
		public List<CompileError> Errors { get; set; }
	}
}