namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a method on a class is missing.
	/// </summary>
	public class MethodOverloadCountError : DefinitionError
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
		/// The number of expected overloads.
		/// </summary>
		public int ExpectedOverloadCount { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodOverloadCountError(
			string className, 
			string expectedMethodName, 
			bool expectedStatic, 
			int expectedOverloadCount)
		{
			ClassName = className;
			ExpectedMethodName = expectedMethodName;
			ExpectedStatic = expectedStatic;
			ExpectedOverloadCount = expectedOverloadCount;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"Expected {ExpectedOverloadCount} {(ExpectedStatic ? "static" : "non-static")} overloaded methods with name '{ExpectedMethodName}'"
			+ ((ClassName != null) 
				? $" on class '{ClassName}'."
				: ".");
	}
}
