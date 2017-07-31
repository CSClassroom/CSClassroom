using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.Interfaces.Extensions;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Generates results for a single user/assignment.
	/// </summary>
	public class AssignmentResultGenerator : IAssignmentResultGenerator
	{
		/// <summary>
		/// The question result generator.
		/// </summary>
		private readonly IQuestionResultGenerator _questionResultGenerator;

		/// <summary>
		/// The assignment score calculator.
		/// </summary>
		private readonly IAssignmentScoreCalculator _assignmentScoreCalculator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentResultGenerator(
			IQuestionResultGenerator questionResultGenerator,
			IAssignmentScoreCalculator assignmentScoreCalculator)
		{
			_questionResultGenerator = questionResultGenerator;
			_assignmentScoreCalculator = assignmentScoreCalculator;
		}

		/// <summary>
		/// Creates an assignment result.
		/// </summary>
		public AssignmentResult CreateAssignmentResult(
			Section section, 
			Assignment assignment, 
			User user, 
			bool admin,
			IList<UserQuestionSubmission> submissions)
		{
			var assignmentName = assignment.Name;
			var assignmentId = assignment.Id;
			var userId = user.Id;
			var dueDate = assignment.GetDueDate(section);
			var totalPoints = assignment.Questions.Sum(q => q.Points);
			var combinedSubmissions = assignment.CombinedSubmissions;
			var showQuestionResults = !assignment.OnlyShowCombinedScore || admin;

			if (combinedSubmissions)
			{
				var assignmentSubmissionResults = GetAssignmentSubmissionResults
				(
					section,
					assignment,
					user,
					showQuestionResults,
					submissions,
					totalPoints
				);

				var score = _assignmentScoreCalculator.GetAssignmentScore
				(
					assignmentSubmissionResults, 
					roundDigits: 2
				);

				var status = _assignmentScoreCalculator.GetAssignmentStatus
				(
					assignmentSubmissionResults, 
					dueDate
				);

				return new CombinedSubmissionsAssignmentResult
				(
					assignmentName,
					assignmentId,
					userId,
					dueDate,
					score,
					totalPoints,
					status,
					assignmentSubmissionResults
				);
			}
			else
			{
				var questionResults = GetQuestionResults(section, assignment, user, submissions);
				var score = _assignmentScoreCalculator.GetAssignmentScore(questionResults, roundDigits: 2);
				var status = _assignmentScoreCalculator.GetAssignmentStatus(questionResults);

				return new SeparateSubmissionsAssignmentResult
				(
					assignmentName,
					assignmentId,
					userId,
					dueDate,
					score,
					totalPoints,
					status,
					showQuestionResults
						? questionResults
						: new List<StudentQuestionResult>()
				);
			}
		}

		/// <summary>
		/// Return a set of results of a user's submissions for a particular assignment.
		/// </summary>
		private IList<AssignmentSubmissionResult> GetAssignmentSubmissionResults(
			Section section,
			Assignment assignment,
			User user,
			bool showQuestionResults,
			IList<UserQuestionSubmission> submissions,
			double totalPoints)
		{
			var questionResultsPerSubmission = submissions
				.Where(s => s.UserQuestionData.AssignmentQuestion.Assignment == assignment)
				.OrderBy(s => s.DateSubmitted)
				.GroupBy(s => s.DateSubmitted)
				.Select
				(
					submissionGroup => new
					{
						DateSubmitted = submissionGroup.Key,
						QuestionResults = GetQuestionResults
						(
							section,
							assignment,
							user,
							submissionGroup.ToList()
						),
					}
				).ToList();

			var scoredSubmissions = questionResultsPerSubmission
				.Select
				(
					submissionQuestionResults => new
					{
						Submission = submissionQuestionResults,
						Score = submissionQuestionResults
							.QuestionResults
							.Sum(result => result.Score),
						TotalPoints = totalPoints,
						Status = _assignmentScoreCalculator.GetAssignmentStatus
						(
							submissionQuestionResults.QuestionResults
						)
					}
				).ToList();

			return scoredSubmissions
				.Select
				(
					scoredSubmission => new AssignmentSubmissionResult
					(
						assignment.Id,
						user.Id,
						scoredSubmission.Submission.DateSubmitted,
						scoredSubmission.Status,
						scoredSubmission.Score,
						scoredSubmission.TotalPoints,
						showQuestionResults
							? scoredSubmission.Submission.QuestionResults
							: new List<StudentQuestionResult>()
					)
				).ToList();
		}

		/// <summary>
		/// Returns the question results for an assignment.
		/// </summary>
		private List<StudentQuestionResult> GetQuestionResults(
			Section section,
			Assignment assignment,
			User user,
			IList<UserQuestionSubmission> submissions)
		{
			var submissionsByQuestion = submissions
				.GroupBy(s => s.UserQuestionData.AssignmentQuestion)
				.ToDictionary(g => g.Key, g => g.ToList());

			var questionResults = assignment.Questions
				.OrderBy(aq => aq.Order)
				.Select
				(
					question => _questionResultGenerator.CreateQuestionResult
					(
						question,
						user,
						submissionsByQuestion.GetValueOrNew(question),
						assignment.GetDueDate(section)
					)
				).ToList();

			return questionResults;
		}
	}
}
