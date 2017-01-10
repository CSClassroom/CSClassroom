namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a class is missing a main method.
	/// </summary>
	public class MainMethodMissingError : DefinitionError
	{
		/// <summary>
		/// The name of the class (if any).
		/// </summary>
		public string ClassName { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MainMethodMissingError(string className)
		{
			ClassName = className;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"No valid main method found on class {ClassName}.";
	}
}
