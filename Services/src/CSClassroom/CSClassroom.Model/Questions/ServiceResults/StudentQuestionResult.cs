namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result for a single problem, for a single student.
	/// </summary>
	public class StudentQuestionResult
	{
		/// <summary>
		/// The question ID.
		/// </summary>
		public int QuestionId { get; }

		/// <summary>
		/// The name of the question.
		/// </summary>
		public string QuestionName { get; }

		/// <summary>
		/// The points the question was worth.
		/// </summary>
		public double QuestionPoints { get; }

		/// <summary>
		/// The score for the question.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The status of the question.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentQuestionResult(
			int questionId,
			string questionName,
			double questionPoints,
			double score,
			SubmissionStatus status)
		{
			QuestionId = questionId;
			QuestionName = questionName;
			QuestionPoints = questionPoints;
			Score = score;
			Status = status;
		}
	}
}
