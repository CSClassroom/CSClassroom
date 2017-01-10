namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error in answering a code question.
	/// </summary>
	public abstract class DefinitionError : CodeQuestionError
	{
		/// <summary>
		/// The line number the error was found on.
		/// </summary>
		public override int? LineNumber => null;

		/// <summary>
		/// The text to show next to the line that has the error,
		/// if the line number is set.
		/// </summary>
		public override string LineErrorText => null;
	}
}
