namespace CSC.BuildService.Model.ProjectRunner
{
	/// <summary>
	/// A test result.
	/// </summary>
	public class TestResult
	{
		/// <summary>
		/// The class name.
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// The name of the test method.
		/// </summary>
		public string TestName { get; set; }

		/// <summary>
		/// The test failure (if any).
		/// </summary>
		public TestFailure Failure { get; set; }

		/// <summary>
		/// Whether or not the tes succeeed.
		/// </summary>
		public bool Succeeded => Failure == null;
	}
}
