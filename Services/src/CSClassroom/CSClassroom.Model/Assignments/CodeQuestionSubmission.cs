namespace CSC.CSClassroom.Model.Assignments
{
	/// <summary>
	/// A submission that contains an answer to a code question.
	/// </summary>
	public class CodeQuestionSubmission : QuestionSubmission
	{
		/// <summary>
		/// The contents of the submission.
		/// </summary>
		public string Contents { get; set; }
	}
}
