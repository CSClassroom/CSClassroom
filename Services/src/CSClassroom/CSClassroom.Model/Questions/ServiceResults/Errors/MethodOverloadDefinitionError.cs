namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that a method on a class is missing.
	/// </summary>
	public class MethodOverloadDefinitionError : DefinitionError
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
		/// Whether or not the method is expected to be public.
		/// </summary>
		public bool ExpectedPublic { get; }

		/// <summary>
		/// The expected parameter types of the overload.
		/// </summary>
		public string ExpectedParamTypes { get; }

		/// <summary>
		/// The expected return types of the overload.
		/// </summary>
		public string ExpectedReturnType { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodOverloadDefinitionError(
			string className, 
			string expectedMethodName, 
			bool expectedStatic, 
			bool expectedPublic,
			string expectedParamTypes,
			string expectedReturnType)
		{
			ClassName = className;
			ExpectedMethodName = expectedMethodName;
			ExpectedStatic = expectedStatic;
			ExpectedPublic = expectedPublic;
			ExpectedParamTypes = expectedParamTypes;
			ExpectedReturnType = expectedReturnType;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText =>
			string.Format("Expected {0} {1} overloaded method with name {2}, {3}, and return type {4}.",
				ExpectedPublic ? "public" : "private",
				ExpectedStatic ? "static" : "non-static",
				ExpectedMethodName,
				string.IsNullOrWhiteSpace(ExpectedParamTypes) ? "no parameters" : $"parameters of type ({ExpectedParamTypes})",
				ExpectedReturnType);
	}
}
