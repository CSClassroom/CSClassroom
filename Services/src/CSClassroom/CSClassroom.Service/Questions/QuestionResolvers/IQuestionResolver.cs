using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionResolvers
{
	/// <summary>
	/// Returns the actual question presented to the user, given
	/// a question object.
	/// </summary>
	public interface IQuestionResolver
	{
		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		Task<Question> ResolveUnsolvedQuestionAsync();

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		Task<Question> ResolveSolvedQuestionAsync(UserQuestionSubmission submission);
	}
}
