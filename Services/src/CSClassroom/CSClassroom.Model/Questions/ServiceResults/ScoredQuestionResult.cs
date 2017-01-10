namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// A scored result of a question submission.
	/// </summary>
	public class ScoredQuestionResult
	{
		/// <summary>
		/// The submission result.
		/// </summary>
		public QuestionResult Result { get; }

		/// <summary>
		/// The resulting score.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ScoredQuestionResult(QuestionResult result, double score)
		{
			Result = result;
			Score = score;
		}
	}
}
