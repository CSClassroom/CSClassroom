namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// A test to run for a class.
	/// </summary>
	public class ClassTest
	{
		/// <summary>
		/// The name of the test.
		/// </summary>
		public string TestName { get; set; }

		/// <summary>
		/// The return type of the test method.
		/// </summary>
		public string ReturnType { get; set; }

		/// <summary>
		/// The body of the test method.
		/// </summary>
		public string MethodBody { get; set; }
	}
}