namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// Represents a java constructor invocation.
	/// </summary>
	public interface IJavaConstructorInvocationGenerator
	{
		/// <summary>
		/// Generates the serializable class.
		/// </summary>
		void GenerateConstructorInvocation(
			object obj, 
			string prefix, 
			string suffix);
	}
}