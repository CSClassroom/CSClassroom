namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// A result of a multiple choice question submission.
	/// </summary>
	public class MultipleChoiceQuestionResult : QuestionResult
	{
		/// <summary>
		/// Whether or not the correct choices were selected.
		/// </summary>
		public bool Correct { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MultipleChoiceQuestionResult(bool correct)
		{
			Correct = correct;
		}
	}
}
