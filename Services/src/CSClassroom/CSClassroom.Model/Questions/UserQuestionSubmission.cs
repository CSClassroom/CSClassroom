using System;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A submission of a question result.
	/// </summary>
	public class UserQuestionSubmission
	{
		/// <summary>
		/// The unique ID for the question result submission score.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the user question data.
		/// </summary>
		public int UserQuestionDataId { get; set; }

		/// <summary>
		/// The question data.
		/// </summary>
		public virtual UserQuestionData UserQuestionData { get; set; }

		/// <summary>
		/// The score of the submission.
		/// </summary>
		public double Score { get; set; }

		/// <summary>
		/// When the submission occured.
		/// </summary>
		public DateTime DateSubmitted { get; set; }

		/// <summary>
		/// The seed used to generate the question (if the question was generated).
		/// </summary>
		public int? Seed { get; set; }

		/// <summary>
		/// Any relevant cached question data for the question grader.
		/// </summary>
		public string CachedQuestionData { get; set; }

		/// <summary>
		/// The serialized question submission.
		/// </summary>
		public string SubmissionContents { get; set; }
	}
}
