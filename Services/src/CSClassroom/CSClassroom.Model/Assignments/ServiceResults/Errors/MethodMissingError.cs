namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a method on a class is missing.
	/// </summary>
	public class MethodMissingError : DefinitionError
	{
		/// <summary>
		/// The name of the class (if any).
		/// </summary>
		public string ClassName { get; }

		/// <summary>
		/// The name of the expected method.
		/// </summary>
		public string ExpectedMethodName { get; }

		/// <summary>
		/// Whether or not the method is expected to be static.
		/// </summary>
		public bool ExpectedStatic { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodMissingError(string className, string expectedMethodName, bool expectedStatic)
		{
			ClassName = className;
			ExpectedMethodName = expectedMethodName;
			ExpectedStatic = expectedStatic;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"Expected {(ExpectedStatic ? "static" : "non-static")} method with name '{ExpectedMethodName}'"
			+ ((ClassName != null) 
				? $" on class '{ClassName}'."
				: ".");
	}
}
