using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Calculates student assignment scores.
	/// </summary>
	public class AssignmentScoreCalculator : IAssignmentScoreCalculator
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
		/// The tolerance for comparing two scores.
		/// </summary>
		private const double c_tolerance = 0.0001;

		/// <summary>
		/// Calculates the score for a single assignment in a single section,
		/// for all students.
		/// </summary>
		public SectionAssignmentResults GetSectionAssignmentResults(
			string assignmentGroupName,
			IList<Assignment> assignments,
			Section section,
			IList<User> users,
			IList<UserQuestionSubmission> submissions)
		{
			var assignmentSubmissions = GetAssignmentSubmissions(assignments, submissions);

			var userAssignmentSubmissions = assignmentSubmissions
				.GroupBy(s => s.UserQuestionData.UserId)
				.ToDictionary(g => g.Key, g => g.ToList());

			var assignmentName = assignments.First().GroupName;
			var sectionName = section.DisplayName;
			var points = assignments.Sum(a => a.Questions.Sum(q => q.Points));
			var studentResults = users.OrderBy(u => u.LastName)
				.ThenBy(u => u.FirstName)
				.Select
				(
					user => GetAssignmentGroupResult
					(
						section,
						user,
						assignmentGroupName,
						assignments,
						userAssignmentSubmissions.ContainsKey(user.Id)
							? GetQuestionResults
								(
									section,
									assignments,
									userAssignmentSubmissions[user.Id]
								)
							: new List<StudentQuestionResult>()
					)
				).ToList();

			return new SectionAssignmentResults
			(
				assignmentName,
				sectionName,
				points,
				studentResults
			);
		}

		/// <summary>
		/// Calculates the scores of all assignments for a given student.
		/// </summary>
		public StudentAssignmentResults GetStudentAssignmentResults(
			User user,
			Section section,
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions)
		{
			return new StudentAssignmentResults
			(
				user.LastName,
				user.FirstName,
				section.DisplayName,
				assignments
					.OrderBy
					(
						assignment => assignment
							.DueDates
							.SingleOrDefault(d => d.SectionId == section.Id)
							.DueDate
					)
					.Select
					(
						assignment => GetAssignmentResult
						(
							section,
							user,
							assignment,
							GetQuestionResults
							(
									section,
									new[] { assignment },
									GetAssignmentSubmissions
									(
										new[] { assignment },
										submissions
									)
							)
						)
					)
					.ToList()
			);
		}

		/// <summary>
		/// Calculates the scores for all assignments updated since the last time 
		/// assignments were marked as graded, for a given section.
		/// </summary>
		public UpdatedSectionAssignmentResults GetUpdatedAssignmentResults(
			IList<Assignment> assignments,
			IList<User> users,
			Section section,
			string gradebookName,
			DateTime lastTransferDate,
			IList<UserQuestionSubmission> submissions)
		{
			var oldAssignmentResults = GetAssignmentResults
			(
				assignments,
				users,
				section,
				submissions,
				lastTransferDate
			);

			var newAssignmentResults = GetAssignmentResults
			(
				assignments,
				users,
				section,
				submissions,
				DateTime.UtcNow
			);
			
			return new UpdatedSectionAssignmentResults
			(
				section.DisplayName,
				gradebookName,
				lastTransferDate,
				GetChangedAssignmentResults
				(
					users,
					oldAssignmentResults,
					newAssignmentResults
				)
			);
		}

		/// <summary>
		/// Returns the same function passed in. This allows us to use 
		/// lambdas with anonymous types.
		/// </summary>
		private Func<TValue, TKey> GetFunc<TValue, TKey>(Func<TValue, TKey> getKey)
		{
			return getKey;
		}

		/// <summary>
		/// Removes unchanged assignment results.
		/// </summary>
		private IList<SectionAssignmentResults> GetChangedAssignmentResults(
			IList<User> users,
			IList<SectionAssignmentResults> oldAssignmentResults, 
			IList<SectionAssignmentResults> newAssignmentResults)
		{
			var getKey = GetFunc
			(
				(SectionAssignmentResult agr) => new
				{
					agr.LastName,
					agr.FirstName,
					agr.AssignmentGroupName
				}
			);

			var oldScores = oldAssignmentResults
				.SelectMany(sar => sar.AssignmentResults)
				.ToDictionary(getKey, agr => agr.Score);

			var includeResult = GetFunc
			(
				(SectionAssignmentResult agr) =>
					!oldScores.ContainsKey(getKey(agr))
					|| Math.Abs(agr.Score - oldScores[getKey(agr)]) > c_tolerance
			);

			return newAssignmentResults
				.Where
				(
					assignmentResults => assignmentResults
						.AssignmentResults
						.Any(includeResult)
				)
				.Select
				(
					assignmentResults => new SectionAssignmentResults
					(
						assignmentResults.AssignmentName,
						assignmentResults.SectionName,
						assignmentResults.Points,
						assignmentResults.AssignmentResults
							.Where(includeResult)
							.ToList()
					)
				).ToList();
		}

		/// <summary>
		/// Returns the assignment results for the given set of submissions.
		/// </summary>
		private IList<SectionAssignmentResults> GetAssignmentResults(
			IList<Assignment> assignments, 
			IList<User> users, 
			Section section, 
			IList<UserQuestionSubmission> submissions,
			DateTime snapshotDate)
		{
			return assignments
				.GroupBy(assignment => assignment.GroupName)
				.Select
				(
					group => new
					{
						GroupName = group.Key,
						DueDate = group.Max
						(
							assignment => assignment.DueDates
								?.SingleOrDefault(dd => dd.SectionId == section.Id)
								?.DueDate ?? DateTime.MinValue
						),
						Questions = new HashSet<int>
						(
							group
								.SelectMany(assignment => assignment.Questions)
								.Select(aq => aq.QuestionId)
						),
						Assignments = group.ToList()
					}
				)
				.Where
				(
					group => group.DueDate != DateTime.MinValue
						&& group.DueDate <= snapshotDate
				)
				.OrderBy
				(
					group => group.DueDate
				)
				.Select
				(
					group => GetSectionAssignmentResults
					(
						group.GroupName,
						group.Assignments,
						section,
						users,
						submissions
							.Where
							(
								submission => submission.DateSubmitted <= snapshotDate && 
								group.Questions.Contains
								(
									submission.UserQuestionData.QuestionId
								)
							)
							.OrderBy(submission => submission.DateSubmitted)
							.ToList()
					)
				).ToList();
		}
		
		/// <summary>
		/// Creates an assignment result, without student information.
		/// </summary>
		private StudentAssignmentResult GetAssignmentResult(
			Section section,
			User user,
			Assignment assignment,
			IList<StudentQuestionResult> questionResults)
		{
			return new StudentAssignmentResult
			(
				assignment.Name,
				assignment.DueDates.Single(d => d.Section == section).DueDate,
				GetAssignmentScore(questionResults, roundDigits: 2),
				questionResults,
				GetAssignmentStatus(questionResults)
			);
		}

		/// <summary>
		/// Creates an assignment result.
		/// </summary>
		private SectionAssignmentResult GetAssignmentGroupResult(
			Section section,
			User user,
			string assignmentGroupName,
			IList<Assignment> assignmentsInGroup,
			IList<StudentQuestionResult> questionResults)
		{
			return new SectionAssignmentResult
			(
				assignmentGroupName,
				user.LastName,
				user.FirstName,
				GetAssignmentScore(questionResults, roundDigits: 1),
				questionResults,
				GetAssignmentStatus(questionResults)
			);
		}

		/// <summary>
		/// Returns the score for an assignment.
		/// </summary>
		private static double GetAssignmentScore(
			IList<StudentQuestionResult> questionResults,
			int roundDigits)
		{
			return Math.Round(questionResults.Sum(qr => qr.Score), roundDigits);
		}

		/// <summary>
		/// Returns the status for an assignment.
		/// </summary>
		private static SubmissionStatus GetAssignmentStatus(
			IList<StudentQuestionResult> questionResults)
		{
			return SubmissionStatus.ForAssignment
			(
				questionResults.Select(qr => qr.Status).ToList()
			);
		}

		/// <summary>
		/// Returns the question results for a group of assignments.
		/// </summary>
		private static List<StudentQuestionResult> GetQuestionResults(
			Section section,
			IList<Assignment> assignmentsInGroup,
			IList<UserQuestionSubmission> submissions)
		{
			var questionSubmissions = submissions
				.GroupBy(s => s.UserQuestionData.Question)
				.ToDictionary(g => g.Key, g => g.ToList());

			var questionResults = assignmentsInGroup
				.Where
				(
					a => a.DueDates?.Any
						 (
							 d => d.Section == section
						 ) ?? false
				)
				.OrderBy
				(
					a => a.DueDates.Single
					(
						d => d.Section == section
					).DueDate
				)
				.SelectMany
				(
					a => a.Questions.OrderBy(aq => aq.Order),
					(assignment, question) => CreateStudentQuestionResult
					(
						question,
						questionSubmissions.ContainsKey(question.Question)
							? questionSubmissions[question.Question]
							: new List<UserQuestionSubmission>(),
						GetDueDate(assignment, section)
					)
				).ToList();

			return questionResults;
		}

		/// <summary>
		/// Returns the section due date for a given assignment.
		/// </summary>
		private static DateTime GetDueDate(Assignment assignment, Section section)
		{
			return assignment
				.DueDates
				.Single(d => d.Section == section)
				.DueDate;
		}

		/// <summary>
		/// Returns all question submissions for the given assignments.
		/// </summary>
		private static IList<UserQuestionSubmission> GetAssignmentSubmissions(
			IEnumerable<Assignment> assignments,
			IList<UserQuestionSubmission> submissions)
		{
			var assignmentQuestions = new HashSet<Question>
			(
				assignments
					.SelectMany(a => a.Questions)
					.Select(aq => aq.Question)
			);

			return submissions.Where
			(
				s => assignmentQuestions.Contains(s.UserQuestionData.Question)
			).ToList();
		}

		/// <summary>
		/// Creates a new student question result.
		/// </summary>
		private static StudentQuestionResult CreateStudentQuestionResult(
			AssignmentQuestion question,
			IList<UserQuestionSubmission> submissions,
			DateTime dueDate)
		{
			var questionId = question.Question.Id;
			var questionName = question.Question.Name;
			var questionPoints = question.Points;
			var score = submissions.Count > 0
				? submissions.Max
					(
						submission => GetSubmissionScore(submission, dueDate, questionPoints)
					)
				: 0.0;

			var dateSubmitted = submissions
				.OrderBy(s => s.DateSubmitted)
				.FirstOrDefault
				(
					s => Math.Abs(GetSubmissionScore(s, dueDate, questionPoints) - score) < c_tolerance
				)?.DateSubmitted;

			var status = SubmissionStatus.ForQuestion(dateSubmitted, dueDate, score);

			return new StudentQuestionResult
			(
				questionId,
				questionName,
				questionPoints,
				score,
				status
			);
		}

		/// <summary>
		/// Returns the score of the submission.
		/// </summary>
		private static double GetSubmissionScore(
			UserQuestionSubmission submission,
			DateTime dueDate,
			double questionPoints)
		{
			var score = submission.Score
				* questionPoints
				* (1 - GetLateDeduction(submission, dueDate));

			return Math.Round(score, 2);
		}


		/// <summary>
		/// Returns the percentage deduction for lateness.
		/// </summary>
		private static double GetLateDeduction(
			UserQuestionSubmission submission,
			DateTime dueDate)
		{
			if (submission.DateSubmitted <= dueDate)
				return 0.0;

			var daysLate = (int)Math.Ceiling
			(
				(submission.DateSubmitted - dueDate).TotalDays
			);

			return Math.Min(daysLate * c_lateDayDeduction, c_maxLateDeduction);
		}
	}
}
