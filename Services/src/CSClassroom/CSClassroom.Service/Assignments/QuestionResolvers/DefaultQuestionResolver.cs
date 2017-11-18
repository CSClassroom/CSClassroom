using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.QuestionResolvers
{
	/// <summary>
	/// A question resolver that just returns the given question.
	/// </summary>
	public class DefaultQuestionResolver : IQuestionResolver
	{
		/// <summary>
		/// The question to resolve.
		/// </summary>
		private readonly UserQuestionData _userQuestionData;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public DefaultQuestionResolver(UserQuestionData userQuestionData)
		{
			_userQuestionData = userQuestionData;
		}

		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		public Task<Question> ResolveUnsolvedQuestionAsync()
		{
			return Task.FromResult(_userQuestionData.AssignmentQuestion.Question);
		}

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		public async Task<Question> ResolveSolvedQuestionAsync(
			UserQuestionSubmission submission)
		{
			// For normal questions, the unsolved question is the same as
			// all solved questions (so we ignore the submission).
			return await ResolveUnsolvedQuestionAsync();
		}
	}
}
