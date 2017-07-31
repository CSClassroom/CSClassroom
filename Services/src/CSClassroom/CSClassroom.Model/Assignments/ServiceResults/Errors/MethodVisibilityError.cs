namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a method does not have the correct visibility.
	/// </summary>
	public class MethodVisibilityError : DefinitionError
	{
		/// <summary>
		/// The name of the method.
		/// </summary>
		public string MethodName { get; }

		/// <summary>
		/// Whether or not the method was expected to be public.
		/// </summary>
		public bool ExpectedPublic { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodVisibilityError(string methodName, bool expectedPublic)
		{
			MethodName = methodName;
			ExpectedPublic = expectedPublic;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"Method '{MethodName}' "
			+ $"{(ExpectedPublic ? "must" : "must not")} be public.";
	}
}
