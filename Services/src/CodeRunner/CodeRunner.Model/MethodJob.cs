using System.Collections.Generic;

namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// A job that compiles a single static method, and runs tests for that method.
	/// </summary>
	public class MethodJob : CodeJob
	{
		/// <summary>
		/// The code for the method (including the signature).
		/// </summary>
		public string MethodCode { get; set; }

		/// <summary>
		/// The tests to run for this class.
		/// </summary>
		public List<MethodTest> Tests { get; set; }
	}
}
