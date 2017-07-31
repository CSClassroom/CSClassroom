namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that the required class was missing.
	/// </summary>
	public class MissingRequiredClassError : DefinitionError
	{
		/// <summary>
		/// The required class name.
		/// </summary>
		public string RequiredClassName { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MissingRequiredClassError(string requiredClassName)
		{
			RequiredClassName = requiredClassName;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText 
			=> $"Submission requires exactly one public class named {RequiredClassName}.";
	}
}
