namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating the method name does not match.
	/// </summary>
	public class MethodNameError : DefinitionError
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodNameError(string expectedMethodName, string actualMethodName)
		{
			ExpectedMethodName = expectedMethodName;
			ActualMethodName = actualMethodName;
		}

		/// <summary>
		/// The expected method name.
		/// </summary>
		public string ExpectedMethodName { get; }

		/// <summary>
		/// The name of the method.
		/// </summary>
		public string ActualMethodName { get; }

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"Method name does not match. "
			+ $"Expected name: '{ExpectedMethodName}'. "
			+ $"Actual name: '{ActualMethodName}'.";
	}
}
