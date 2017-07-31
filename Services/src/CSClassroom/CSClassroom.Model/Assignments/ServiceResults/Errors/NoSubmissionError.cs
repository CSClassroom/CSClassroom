namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that there was no submission.
	/// </summary>
	public class NoSubmissionError : DefinitionError
	{
		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"The submission cannot be blank.";
	}
}
