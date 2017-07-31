using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.QuestionResolvers
{
	/// <summary>
	/// Creates a question resolver for a given UserQuestionData object.
	/// </summary>
	public interface IQuestionResolverFactory
	{
		/// <summary>
		/// Returns the user question updater for the type of question
		/// that corresponds to the given UserQuestionData object.
		/// </summary>
		IQuestionResolver CreateQuestionResolver(UserQuestionData userQuestionData);
	}
}
