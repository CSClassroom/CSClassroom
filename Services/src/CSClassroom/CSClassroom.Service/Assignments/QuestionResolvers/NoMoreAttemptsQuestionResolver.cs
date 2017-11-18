using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.QuestionResolvers
{
	/// <summary>
	/// A question resolver for a question that a user may not make
	/// further attempts to solve.
	/// </summary>
	public class NoMoreAttemptsQuestionResolver : IQuestionResolver
	{
		/// <summary>
		/// The question resolver to resolve the question for previous submissions.
		/// </summary>
		private readonly IQuestionResolver _prevSubmissionResolver;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public NoMoreAttemptsQuestionResolver(IQuestionResolver prevSubmissionResolver)
		{
			_prevSubmissionResolver = prevSubmissionResolver;
		}
		
		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		public Task<Question> ResolveUnsolvedQuestionAsync()
		{
			return Task.FromResult<Question>(null);
		}

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		public async Task<Question> ResolveSolvedQuestionAsync(UserQuestionSubmission submission)
		{
			return await _prevSubmissionResolver.ResolveSolvedQuestionAsync(submission);
		}
	}
}