namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a method appears too few or too many times.
	/// </summary>
	public class MethodCountError : DefinitionError
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
		public int ExpectedCount { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodCountError(
			string className, 
			string expectedMethodName, 
			bool expectedStatic, 
			int expectedCount)
		{
			ClassName = className;
			ExpectedMethodName = expectedMethodName;
			ExpectedStatic = expectedStatic;
			ExpectedCount = expectedCount;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => 
			$"Expected {ExpectedCount}"
			+ $" {(ExpectedStatic ? "static" : "non-static")}"
			+ $" {(ExpectedCount > 1 ? "methods" : "method")}"
			+ $" with name '{ExpectedMethodName}'"
			+ ((ClassName != null) 
				? $" on class '{ClassName}'."
				: ".");
	}
}
