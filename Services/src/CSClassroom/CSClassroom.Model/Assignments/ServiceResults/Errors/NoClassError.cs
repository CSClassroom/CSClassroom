namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a method on a class is missing.
	/// </summary>
	public class NoClassError : DefinitionError
	{
		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"No class found in submission.";
	}
}
