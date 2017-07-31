namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that public fields were present on a class, when they are prohibited.
	/// </summary>
	public class FieldVisibilityError : DefinitionError
	{
		/// <summary>
		/// The name of the class.
		/// </summary>
		public string ClassName { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public FieldVisibilityError(string className)
		{
			ClassName = className;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"Class '{ClassName}' may not have public fields.";
	}
}
