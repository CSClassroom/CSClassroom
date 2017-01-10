namespace CSC.BuildService.Model.ProjectRunner
{
	/// <summary>
	/// Information about a test failure.
	/// </summary>
	public class TestFailure
	{
		/// <summary>
		/// The failure message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The stack trace of the failure.
		/// </summary>
		public string Trace { get; set; }

		/// <summary>
		/// The contents of stdout/stderr when the failure occured.
		/// </summary>
		public string Output { get; set; }
	}
}
