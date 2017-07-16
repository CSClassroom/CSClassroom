using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Service.Questions.UserQuestionDataLoaders
{
	/// <summary>
	/// Creates user question data loaders. 
	/// </summary>
	public interface IUserQuestionDataLoaderFactory
	{
		/// <summary>
		/// Creates a loader that loads the user question data for 
		/// a single question.
		/// </summary>
		IUserQuestionDataLoader CreateLoaderForSingleQuestion(
			string classroomName,
			int assignmentId,
			int assignmentQuestionId,
			int userId);

		/// <summary>
		/// Creates a loader that loads the user question data for 
		/// all questions in the assignment.
		/// </summary>
		IUserQuestionDataLoader CreateLoaderForAllAssignmentQuestions(
			string classroomName,
			int assignmentId,
			int userId);
	}
}
