using System.Collections.Generic;

namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// A job that compiles a class, and runs tests for that class.
	/// </summary>
	public class ClassJob : CodeJob
	{
		/// <summary>
		/// The name of the public class in the file.
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// The contents of the java file.
		/// </summary>
		public string FileContents { get; set; }

		/// <summary>
		/// The offset to apply to line numbers. 
		/// </summary>
		public int LineNumberOffset { get; set; }

		/// <summary>
		/// The tests to run for this class.
		/// </summary>
		public List<ClassTest> Tests { get; set; }
	}
}
