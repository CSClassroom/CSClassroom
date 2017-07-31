using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating that the parameter types of a method do not match.
	/// </summary>
	public class MethodParameterTypesError : DefinitionError
	{
		/// <summary>
		/// The name of the method.
		/// </summary>
		public string MethodName { get; }

		/// <summary>
		/// The expected parameter types.
		/// </summary>
		public IList<string> ExpectedParamTypes { get; }

		/// <summary>
		/// The actual parameter types.
		/// </summary>
		public IList<string> ActualParamTypes { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodParameterTypesError(string methodName, IList<string> expectedParamTypes, List<string> actualParamTypes)
		{
			MethodName = methodName;
			ExpectedParamTypes = expectedParamTypes;
			ActualParamTypes = actualParamTypes;
		}

		/// <summary>
		/// A string with the concatenated expected para
		/// </summary>
		private string ExpectedParamTypesStr => string.Join(",", ExpectedParamTypes);

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => $"Method parameter types does not match. "
			+ $"Expected types: ({string.Join(",", ExpectedParamTypes)}). "
			+ $"Actual types: ({string.Join(",", ActualParamTypes)}).";
	}
}
