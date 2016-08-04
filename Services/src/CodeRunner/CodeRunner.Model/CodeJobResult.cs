using System.Collections.Generic;

namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// The result of a code job.
	/// </summary>
	public abstract class CodeJobResult
	{
		/// <summary>
		/// The status of the code job.
		/// </summary>
		public CodeJobStatus Status { get; set; }

		/// <summary>
		/// Diagnostic output.
		/// </summary>
		public string DiagnosticOutput { get; set; }

		/// <summary>
		/// The compilation result of the class.
		/// </summary>
		public CompilationResult ClassCompilationResult { get; set; }

		/// <summary>
		/// The compilation result for the tests, if the class compiled successfully.
		/// </summary>
		public CompilationResult TestsCompilationResult { get; set; }

		/// <summary>
		/// The results of the tests, if the tests compiled successfully.
		/// </summary>
		public List<CodeTestResult> TestResults { get; set; }
	}
}
