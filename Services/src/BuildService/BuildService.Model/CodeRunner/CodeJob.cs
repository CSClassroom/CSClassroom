using System.Collections.Generic;

namespace CSC.BuildService.Model.CodeRunner
{
	/// <summary>
	/// A job that compiles and tests code.
	/// </summary>
	public abstract class CodeJob
	{
		/// <summary>
		/// The list of classes to import (possibly including wildcards).
		/// </summary>
		public List<string> ClassesToImport { get; set; }
	}
}
