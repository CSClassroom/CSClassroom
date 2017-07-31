namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// A compilation error that occured when compiling the tests for the submitted class.
	/// </summary>
	public class TestCompilationError : CodeQuestionError
	{
		/// <summary>
		/// The error message, as reported by the compiler.
		/// </summary>
		public string CompilerErrorMessage { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TestCompilationError(string compilerErrorMessage)
		{
			CompilerErrorMessage = compilerErrorMessage;
		}

		/// <summary>
		/// The line number the error was found on.
		/// </summary>
		public override int? LineNumber => null;

		/// <summary>
		/// The text to show next to the line that has the error,
		/// if the line number is set.
		/// </summary>
		public override string LineErrorText => null;

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => 
			"The following error occured compiling the tests for this question.\n"
			+ "Ensure your classes and methods are exactly as described.\n"
			+ CompilerErrorMessage;
	}
}
