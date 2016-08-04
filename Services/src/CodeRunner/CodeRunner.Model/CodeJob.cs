using System.Collections.Generic;

namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// A job that compiles and tests code.
	/// </summary>
	public class CodeJob
	{
		/// <summary>
		/// The list of classes to import (possibly including wildcards).
		/// </summary>
		public List<string> ClassesToImport { get; set; }
	}
}
