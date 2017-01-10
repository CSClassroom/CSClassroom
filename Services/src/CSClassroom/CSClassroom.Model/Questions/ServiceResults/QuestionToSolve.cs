using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// A question to solve.
	/// </summary>
	public class QuestionToSolve
	{
		/// <summary>
		/// The question to solve.
		/// </summary>
		public Question Question { get; }

		/// <summary>
		/// The most recent submission.
		/// </summary>
		public QuestionSubmission LastSubmission { get; }

		/// <summary>
		/// The list of unsolved prerequisites, if any.
		/// </summary>
		public IList<Question> UnsolvedPrerequisites { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionToSolve(
			Question question, 
			QuestionSubmission questionSubmission, 
			IList<Question> unsolvedPrereqs)
		{
			Question = question;
			LastSubmission = questionSubmission;
			UnsolvedPrerequisites = unsolvedPrereqs;

		}
	}
}
