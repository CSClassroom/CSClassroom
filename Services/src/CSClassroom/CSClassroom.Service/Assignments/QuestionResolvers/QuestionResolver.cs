using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.QuestionResolvers
{
	/// <summary>
	/// Returns the actual question presented to the user, given
	/// a question object.
	/// </summary>
	public abstract class QuestionResolver : IQuestionResolver
	{
		/// <summary>
		/// The question to resolve.
		/// </summary>
		protected UserQuestionData UserQuestionData { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected QuestionResolver(UserQuestionData userQuestionData)
		{
			UserQuestionData = userQuestionData;
		}

		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		public async Task<Question> ResolveUnsolvedQuestionAsync()
		{
			if (!UserQuestionData.AnyAttemptsRemaining)
			{
				return null;
			}

			return await ResolveUnsolvedQuestionImplAsync();
		}

		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		protected abstract Task<Question> ResolveUnsolvedQuestionImplAsync();

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		public abstract Task<Question> ResolveSolvedQuestionAsync(
			UserQuestionSubmission submission);
	}
}
