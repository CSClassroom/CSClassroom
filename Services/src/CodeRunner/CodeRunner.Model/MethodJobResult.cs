using System.Collections.Generic;

namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// The result of a method job.
	/// </summary>
	public class MethodJobResult : CodeJobResult
	{
		/// <summary>
		/// The defintion of the compiled method, if there was exactly one method 
		/// and it compiled successfully.
		/// </summary>
		public MethodDefinition MethodDefinition { get; set; }
	}
}
