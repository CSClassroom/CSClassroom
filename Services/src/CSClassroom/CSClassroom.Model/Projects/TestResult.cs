namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A test result.
	/// </summary>
	public class TestResult
	{
		/// <summary>
		/// The primary key.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the build that this test result belongs to.
		/// </summary>
		public int BuildId { get; set; }

		/// <summary>
		/// The build.
		/// </summary>
		public Build Build { get; set; }

		/// <summary>
		/// The class name.
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// The name of the test method.
		/// </summary>
		public string TestName { get; set; }

		/// <summary>
		/// Whether or not the test succeeed.
		/// </summary>
		public bool Succeeded { get; set; }

		/// <summary>
		/// Whether or not the test previously succeeded.
		/// </summary>
		public bool PreviouslySucceeded { get; set; }

		/// <summary>
		/// The failure message (if any).
		/// </summary>
		public string FailureMessage { get; set; }

		/// <summary>
		/// The stack trace of the failure (if any).
		/// </summary>
		public string FailureTrace { get; set; }

		/// <summary>
		/// The contents of stdout/stderr when the failure occured (if any).
		/// </summary>
		public string FailureOutput { get; set; }
	}
}
