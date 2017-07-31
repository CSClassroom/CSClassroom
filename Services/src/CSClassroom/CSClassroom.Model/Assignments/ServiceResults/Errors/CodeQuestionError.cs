namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error in answering a code question.
	/// </summary>
	public abstract class CodeQuestionError
	{
		/// <summary>
		/// The line number the error was found on.
		/// </summary>
		public abstract int? LineNumber { get; }

		/// <summary>
		/// The text to show next to the line that has the error,
		/// if the line number is set.
		/// </summary>
		public abstract string LineErrorText { get; }

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public abstract string FullErrorText { get; }
	}
}
