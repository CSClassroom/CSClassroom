using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Extensions;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions.QuestionGraders
{
	/// <summary>
	/// Grades a short answer question.
	/// </summary>
	public class ShortAnswerQuestionGrader : QuestionGrader<ShortAnswerQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ShortAnswerQuestionGrader(ShortAnswerQuestion question) 
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
			var shortSubmission = submission as ShortAnswerQuestionSubmission;
			if (shortSubmission == null)
				throw new ArgumentException("Invalid submission type", nameof(submission));

			var results = new List<bool>();
			var blanks = new HashSet<ShortAnswerQuestionBlank>(Question.Blanks);

			foreach (var submissionBlank in shortSubmission.Blanks)
			{
				var foundBlank = blanks.Single(blank => blank.Name == submissionBlank.Name);

				results.Add
				(
					   submissionBlank.Answer != null 
					&& foundBlank.Answer.TrimEveryLine() == submissionBlank.Answer.TrimEveryLine()
				);

				blanks.Remove(foundBlank);
			}

			if (blanks.Any())
			{
				throw new InvalidOperationException("Submission did not contain all blanks.");
			}

			return new ScoredQuestionResult
			(
				new ShortAnswerQuestionResult(results.ToArray()),
				Question.AllowPartialCredit
					? (results.Count(c => c)*1.0)/results.Count
					: results.All(c => c) ? 1.0 : 0.0
			);
		}
	}
}
