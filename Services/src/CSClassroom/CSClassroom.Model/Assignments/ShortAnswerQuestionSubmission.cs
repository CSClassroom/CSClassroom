using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Assignments
{
	/// <summary>
	/// A submitted choice
	/// </summary>
	public class SubmissionBlank
	{
		/// <summary>
		/// The name of the blank.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The answer in the blank.
		/// </summary>
		public string Answer { get; set; }
	}

	/// <summary>
	/// A submission that contains an answer to a short answer question.
	/// </summary>
	public class ShortAnswerQuestionSubmission : QuestionSubmission
	{
		/// <summary>
		/// The selected blanks.
		/// </summary>
		public List<SubmissionBlank> Blanks { get; set; }
	}
}
