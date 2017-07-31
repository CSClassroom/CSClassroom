namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// A compilation error that occured when compiling the submitted class.
	/// </summary>
	public class ClassCompilationError : CodeQuestionError
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassCompilationError(int lineNumber, string lineErrorText, string fullErrorText)
		{
			LineNumber = lineNumber;
			LineErrorText = lineErrorText;
			FullErrorText = fullErrorText;
		}

		/// <summary>
		/// The line number the error was found on.
		/// </summary>
		public override int? LineNumber { get; }

		/// <summary>
		/// The text to show next to the line that has the error,
		/// if the line number is set.
		/// </summary>
		public override string LineErrorText { get; }

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText { get; }
	}
}
