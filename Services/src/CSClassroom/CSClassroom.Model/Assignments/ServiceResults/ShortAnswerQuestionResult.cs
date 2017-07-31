namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// A result of a multiple choice question submission.
	/// </summary>
	public class ShortAnswerQuestionResult : QuestionResult
	{
		/// <summary>
		/// Whether or not each blank was correct.
		/// </summary>
		public bool[] Correct { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ShortAnswerQuestionResult(bool[] correct)
		{
			Correct = correct;
		}
	}
}
