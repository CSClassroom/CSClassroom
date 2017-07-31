using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A submission that contains an answer to a multiple choice question.
	/// </summary>
	public class MultipleChoiceQuestionSubmission : QuestionSubmission
	{
		/// <summary>
		/// The selected choices.
		/// </summary>
		public List<string> SelectedChoices { get; set; }
	}
}
