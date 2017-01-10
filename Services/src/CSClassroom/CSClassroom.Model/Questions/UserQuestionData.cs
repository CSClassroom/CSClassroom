using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A submission of a question result.
	/// </summary>
	public class UserQuestionData
	{
		/// <summary>
		/// The unique ID for the question result submission.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the user who submitted the result.
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// The question.
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// The question this is a submission for.
		/// </summary>
		public int QuestionId { get; set; }

		/// <summary>
		/// The question.
		/// </summary>
		public Question Question { get; set; }

		/// <summary>
		/// The number of attempts.
		/// </summary>
		public int NumAttempts { get; set; }

		/// <summary>
		/// The serialized question submission.
		/// </summary>
		public string LastQuestionSubmission { get; set; }

		/// <summary>
		/// The seed used to generate the question (if the question was generated).
		/// </summary>
		public int? Seed { get; set; }

		/// <summary>
		/// Any relevant cached question data for the question grader.
		/// </summary>
		public string CachedQuestionData { get; set; }

		/// <summary>
		/// When the cached question data was generated.
		/// </summary>
		public DateTime? CachedQuestionDataTime { get; set; }

		/// <summary>
		/// The scores of each attempted submission.
		/// </summary>
		public ICollection<UserQuestionSubmission> Submissions { get; set; }
	}
}
