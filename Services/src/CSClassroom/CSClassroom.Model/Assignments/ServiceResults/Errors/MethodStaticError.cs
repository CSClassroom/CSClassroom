namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a method should should have been 
	/// static but wasn't (or vice versa).
	/// </summary>
	public class MethodStaticError : DefinitionError
	{
		/// <summary>
		/// The name of the method.
		/// </summary>
		public string MethodName { get; }

		/// <summary>
		/// Whether or not the method was expected to be static.
		/// </summary>
		public bool ExpectedStatic { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodStaticError(string methodName, bool expectedStatic)
		{
			MethodName = methodName;
			ExpectedStatic = expectedStatic;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"Method '{MethodName}' "
			+ $"{(ExpectedStatic ? "must" : "must not")} be static.";
	}
}
