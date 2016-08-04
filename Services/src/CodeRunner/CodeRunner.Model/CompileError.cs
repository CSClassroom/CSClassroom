using System.Collections.Generic;

namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// An error encountered during compilation.
	/// </summary>
	public class CompileError
	{
		/// <summary>
		/// The line number of the error.
		/// </summary>
		public int LineNumber { get; set; }

		/// <summary>
		/// The column number of the error.
		/// </summary>
		public int ColumnNumber { get; set; }

		/// <summary>
		/// The error message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The full error message, including context.
		/// </summary>
		public string FullError { get; set; }
	}
}