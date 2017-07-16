using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;
using MoreLinq;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Generates a result for a single question completed by a student.
	/// </summary>
	public class QuestionResultGenerator : IQuestionResultGenerator
	{
		/// <summary>
		/// The question score calculator.
		/// </summary>
		private readonly IQuestionScoreCalculator _questionScoreCalculator;

		/// <summary>
		/// The submission status calculator.
		/// </summary>
		private readonly ISubmissionStatusCalculator _submissionStatusCalculator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionResultGenerator(
			IQuestionScoreCalculator questionScoreCalculator,
			ISubmissionStatusCalculator submissionStatusCalculator)
		{
			_questionScoreCalculator = questionScoreCalculator;
			_submissionStatusCalculator = submissionStatusCalculator;
		}

		/// <summary>
		/// Creates a new student question result.
		/// </summary>
		public StudentQuestionResult CreateQuestionResult(
			AssignmentQuestion question,
			User user,
			IList<UserQuestionSubmission> submissions,
			DateTime? dueDate)
		{
			var assignmentQuestionId = question.Id;
			var assignmentId = question.AssignmentId;
			var userId = user.Id;
			var combinedSubmissions = question.Assignment.CombinedSubmissions;
			var questionName = question.Name;
			var questionPoints = question.Points;
			var scoredSubmissions = submissions
				.OrderBy(submission => submission.DateSubmitted)
				.Select
				(
					submission => new
					{
						Submission = submission,
						Score = _questionScoreCalculator.GetSubmissionScore
						(
							submission,
							dueDate,
							questionPoints,
							withLateness: true
						)
					}
				).ToList();

			var bestSubmission = submissions.Count > 0
				? scoredSubmissions.MaxBy(result => result.Score)
				: null;

			var score = bestSubmission?.Score ?? 0.0;
			var dateSubmitted = bestSubmission?.Submission?.DateSubmitted;
			var status = _submissionStatusCalculator.GetStatusForQuestion
			(
				dateSubmitted,
				dueDate,
				question.IsInteractive(),
				score
			);

			bool includeSubmissions = !question.IsInteractive() 
				&& !question.Assignment.CombinedSubmissions;

			var submissionResults = includeSubmissions
				? scoredSubmissions.Select
					(
						scoredSubmission => new QuestionSubmissionResult
						(
							question.Id,
							question.AssignmentId,
							userId,
							scoredSubmission.Submission.DateSubmitted,
							_submissionStatusCalculator.GetStatusForQuestion
							(
								scoredSubmission.Submission.DateSubmitted,
								dueDate,
								false /*interactive*/,
								scoredSubmission.Score
							),
							scoredSubmission.Score,
							question.Points
						)
					).ToList()
				: null;

			return new StudentQuestionResult
			(
				assignmentQuestionId,
				assignmentId,
				userId,
				combinedSubmissions,
				questionName,
				questionPoints,
				score,
				status,
				submissionResults
			);
		}
	}
}
