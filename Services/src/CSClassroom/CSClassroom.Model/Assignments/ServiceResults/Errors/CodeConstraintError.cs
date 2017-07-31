namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a submission does not adhere to a constraint.
	/// </summary>
	public class CodeConstraintError : DefinitionError
	{
		/// <summary>
		/// The regex found too many or too few times
		/// </summary>
		public string Regex { get; }

		/// <summary>
		/// The frequency of the regex in the submission
		/// </summary>
		public int ActualFrequency { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CodeConstraintError(string regex, int actualFrequency, string fullErrorText)
		{
			Regex = regex;
			ActualFrequency = actualFrequency;
			FullErrorText = fullErrorText;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText { get; }
	}
}
