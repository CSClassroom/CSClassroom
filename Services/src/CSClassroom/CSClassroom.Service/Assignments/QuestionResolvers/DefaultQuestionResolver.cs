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
	public class DefaultQuestionResolver : QuestionResolver
	{
		public DefaultQuestionResolver(UserQuestionData userQuestionData)
			: base(userQuestionData)
		{
		}

		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		protected override Task<Question> ResolveUnsolvedQuestionImplAsync()
		{
			return Task.FromResult(UserQuestionData.AssignmentQuestion.Question);
		}

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		public override async Task<Question> ResolveSolvedQuestionAsync(
			UserQuestionSubmission submission)
		{
			// For normal questions, the unsolved question is the same as
			// all solved questions (so we ignore the submission).
			return await ResolveUnsolvedQuestionImplAsync();
		}
	}
}
