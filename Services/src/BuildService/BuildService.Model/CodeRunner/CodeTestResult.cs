namespace CSC.BuildService.Model.CodeRunner
{
	/// <summary>
	/// The result of a test.
	/// </summary>
	public class CodeTestResult
	{
		/// <summary>
		/// The name of the test.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Whether or not the test completed successfully.
		/// </summary>
		public bool Completed { get; set; }

		/// <summary>
		/// The exception, if the test did not complete successfully.
		/// </summary>
		public string Exception { get; set; }

		/// <summary>
		/// The return value of the test, if the test method had a non-void return type.
		/// </summary>
		public string ReturnValue { get; set; }

		/// <summary>
		/// The output of the test, if any.
		/// </summary>
		public string Output { get; set; }
	}
}