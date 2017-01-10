using System;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions.QuestionGraders
{
	/// <summary>
	/// Grades a multiple choice question.
	/// </summary>
	public class MultipleChoiceQuestionGrader : QuestionGrader<MultipleChoiceQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MultipleChoiceQuestionGrader(MultipleChoiceQuestion question) 
			: base(question)
		{
		}

		/// <summary>
		/// Grades the question submission.
		/// </summary>
		public override Task<ScoredQuestionResult> GradeSubmissionAsync(QuestionSubmission submission)
		{
			return Task.FromResult(GradeSubmission(submission));
		}

		/// <summary>
		/// Grades the question submission.
		/// </summary>
		private ScoredQuestionResult GradeSubmission(QuestionSubmission submission)
		{
			var multSubmission = submission as MultipleChoiceQuestionSubmission;
			if (multSubmission == null)
				throw new ArgumentException("Invalid submission type", nameof(submission));

			var result = new MultipleChoiceQuestionResult
			(
				Question.Choices.All
				(
					choice => choice.Correct == multSubmission.SelectedChoices.Contains(choice.Value)
				)
			);

			return new ScoredQuestionResult(result, result.Correct ? 1.0 : 0.0);
		}
	}
}
