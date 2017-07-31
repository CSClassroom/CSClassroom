namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating the return type of a method does not match.
	/// </summary>
	public class MethodReturnTypeError : DefinitionError
	{
		/// <summary>
		/// The name of the method.
		/// </summary>
		public string MethodName { get; }

		/// <summary>
		/// The expected return type.
		/// </summary>
		public string ExpectedReturnType { get; }

		/// <summary>
		/// The actual return type.
		/// </summary>
		public string ActualReturnType { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodReturnTypeError(string methodName, string expectedReturnType, string actualReturnType)
		{
			MethodName = methodName;
			ExpectedReturnType = expectedReturnType;
			ActualReturnType = actualReturnType;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => "Method return type does not match. " 
			+ $"Expected type: '{ExpectedReturnType}'. "
			+ $"Actual type: '{ActualReturnType}'.";
	}
}
