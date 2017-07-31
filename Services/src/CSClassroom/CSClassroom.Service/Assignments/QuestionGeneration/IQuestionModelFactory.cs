using System.Collections.Generic;

namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// Creates the question object model.
	/// </summary>
	public interface IQuestionModelFactory
	{
		/// <summary>
		/// Returns the question object model.
		/// </summary>
		IList<JavaClass> GetQuestionModel();
	}
}
