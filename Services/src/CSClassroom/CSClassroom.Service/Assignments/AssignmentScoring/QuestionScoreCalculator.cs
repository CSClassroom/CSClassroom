using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Returns the total score received for a question.
	/// </summary>
	public class QuestionScoreCalculator : IQuestionScoreCalculator
	{
		/// <summary>
		/// The maximum lateness deduction.
		/// </summary>
		private const double c_maxLateDeduction = 0.20;

		/// <summary>
		/// The deduction for each day late.
		/// </summary>
		private const double c_lateDayDeduction = 0.05;

		/// <summary>
		/// Returns the score of the submission.
		/// </summary>
		public double GetSubmissionScore(
			UserQuestionSubmission submission,
			DateTime? dueDate,
			double questionPoints,
			bool withLateness)
		{
			var score = submission.Score * questionPoints;
			if (withLateness)
			{
				score *= (1 - GetLateDeduction(submission, dueDate));
			}

			return Math.Round(score, 2);
		}

		/// <summary>
		/// Returns the percentage deduction for lateness.
		/// </summary>
		private double GetLateDeduction(
			UserQuestionSubmission submission,
			DateTime? dueDate)
		{
			if (!dueDate.HasValue || submission.DateSubmitted <= dueDate)
				return 0.0;

			var daysLate = (int)Math.Ceiling
			(
				(submission.DateSubmitted - dueDate.Value).TotalDays
			);

			return Math.Min(daysLate * c_lateDayDeduction, c_maxLateDeduction);
		}
	}
}
