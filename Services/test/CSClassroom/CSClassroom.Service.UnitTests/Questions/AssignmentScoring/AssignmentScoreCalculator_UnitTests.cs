using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Questions.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the AssignmentResultGenerator class.
	/// </summary>
	public class AssignmentScoreCalculator_UnitTests
	{
		/// <summary>
		/// An example question due date.
		/// </summary>
		private static readonly DateTime DueDate = new DateTime(2016, 1, 1, 12, 0, 0);

		/// <summary>
		/// Ensures that GetSectionAssignmentResults returns the correct results.
		/// </summary>
		[Fact]
		public void GetSectionAssignmentResults_CorrectResults()
		{
			var users = GetUsers();
			var questions = GetQuestions();
			var sections = GetSections();
			var assignments = GetAssignments(sections, questions);
			var submissions = GetSubmissions(users, questions);

			var scoreCalculator = new AssignmentScoreCalculator();
			var expectedResults = GetExpectedSectionAssignmentResults();
			var actualResults = scoreCalculator.GetSectionAssignmentResults
			(
				"Unit 1",
				assignments,
				sections[0],
				users,
				submissions
			);

			VerifySectionAssignmentResults(expectedResults, actualResults);
		}

		/// <summary>
		/// Ensures that GetStudentAssignmentResults returns the correct results.
		/// </summary>
		[Fact]
		public void GetStudentAssignmentResults_CorrectResults()
		{
			var users = GetUsers();
			var questions = GetQuestions();
			var sections = GetSections();
			var assignments = GetAssignments(sections, questions);
			var submissions = GetSubmissions(users, questions);

			var scoreCalculator = new AssignmentScoreCalculator();
			var expectedResults = GetExpectedStudentAssignmentResults();
			var actualResults = scoreCalculator.GetStudentAssignmentResults
			(
				users[0],
				sections[0],
				assignments,
				submissions
			);

			VerifyStudentAssignmentResults(expectedResults, actualResults);
		}

		/// <summary>
		/// Ensures that GetUpdatedAssignmentResults returns the correct results.
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentResults_CorrectResults()
		{
			var users = GetUsers();
			var questions = GetQuestions();
			var sections = GetSections();
			var assignments = GetAssignments(sections, questions);
			var submissions = GetSubmissions(users, questions);

			var scoreCalculator = new AssignmentScoreCalculator();
			var expectedResults = GetExpectedUpdatedAssignmentResults();
			var actualResults = scoreCalculator.GetUpdatedAssignmentResults
			(
				assignments,
				users,
				sections[0],
				"Gradebook1",
				DueDate,
				submissions
			);

			VerifyUpdatedAssignmentResults(expectedResults, actualResults);
		}

		/// <summary>
		/// Returns a list of users.
		/// </summary>
		private IList<User> GetUsers()
		{
			return Collections.CreateList
			(
				new User() { Id = 1, LastName = "Last1", FirstName = "First1" },
				new User() { Id = 2, LastName = "Last2", FirstName = "First2" },
				new User() { Id = 3, LastName = "Last3", FirstName = "First3" }
			);
		}

		/// <summary>
		/// Returns a list of sections.
		/// </summary>
		private IList<Section> GetSections()
		{
			return Collections.CreateList
			(
				new Section() { Id = 1, DisplayName = "Period1" },
				new Section() { Id = 2, DisplayName = "Period2" }
			);
		}

		/// <summary>
		/// Returns a list of questions.
		/// </summary>
		private IList<Question> GetQuestions()
		{
			return Collections.CreateList<Question>
			(
				new MethodQuestion() { Id = 1, Name = "Question1" },
				new MethodQuestion() { Id = 2, Name = "Question2" },
				new MethodQuestion() { Id = 3, Name = "Question3" },
				new MethodQuestion() { Id = 4, Name = "Question4" }
			);
		}

		/// <summary>
		/// Returns a list of assignments.
		/// </summary>
		private IList<Assignment> GetAssignments(
			IList<Section> sections, 
			IList<Question> questions)
		{
			return Collections.CreateList
			(
				new Assignment()
				{
					Name = "Unit 1a",
					GroupName = "Unit 1",
					DueDates = GetDueDates(sections),
					Questions = GetAssignmentQuestions(questions.Take(3)),
				},
				new Assignment()
				{
					Name = "Unit 1b",
					GroupName = "Unit 1",
					DueDates = GetDueDates(sections),
					Questions = GetAssignmentQuestions(questions.Skip(3))
				}
			);
		}

		/// <summary>
		/// Returns a list of assignment questions.
		/// </summary>
		public IList<AssignmentQuestion> GetAssignmentQuestions(IEnumerable<Question> questions)
		{
			return questions
				.Select
				(
					question => new AssignmentQuestion()
					{
						Question = question,
						QuestionId = question.Id,
						Points = 1.0
					}
				).ToList();
		}

		/// <summary>
		/// Returns a list of due dates.
		/// </summary>
		private IList<AssignmentDueDate> GetDueDates(IList<Section> sections)
		{
			return Collections.CreateList
			(
				new AssignmentDueDate()
				{
					Section = sections[0],
					SectionId = sections[0].Id,
					DueDate = DueDate
				},
				new AssignmentDueDate()
				{
					Section = sections[1],
					SectionId = sections[1].Id,
					DueDate = DueDate
				}
			);
		}

		/// <summary>
		/// Returns a list of question submissions.
		/// </summary>
		private IList<UserQuestionSubmission> GetSubmissions(
			IList<User> users, 
			IList<Question> questions)
		{
			return Collections.CreateList
			(
				GetUserQuestionSubmission(users[0], questions[0], 0.0, DueDate - TimeSpan.FromDays(1)),
				GetUserQuestionSubmission(users[0], questions[0], 1.0, DueDate - TimeSpan.FromDays(1)),
				GetUserQuestionSubmission(users[0], questions[0], 1.0, DueDate + TimeSpan.FromHours(1)),
				GetUserQuestionSubmission(users[0], questions[1], 1.0, DueDate + TimeSpan.FromHours(50)),
				GetUserQuestionSubmission(users[0], questions[2], 1.0, DueDate - TimeSpan.FromHours(50)),
				GetUserQuestionSubmission(users[0], questions[3], 1.0, DueDate + TimeSpan.FromHours(50)),
				GetUserQuestionSubmission(users[1], questions[0], 0.0, DueDate - TimeSpan.FromDays(1)),
				GetUserQuestionSubmission(users[1], questions[0], 1.0, DueDate - TimeSpan.FromDays(1)),
				GetUserQuestionSubmission(users[1], questions[2], 0.0, DueDate + TimeSpan.FromDays(1)),
				GetUserQuestionSubmission(users[2], questions[0], 0.0, DueDate - TimeSpan.FromDays(1)),
				GetUserQuestionSubmission(users[2], questions[0], 1.0, DueDate - TimeSpan.FromDays(1))
			);
		}

		/// <summary>
		/// Returns a new user question submission.
		/// </summary>
		private UserQuestionSubmission GetUserQuestionSubmission(
			User user,
			Question question,
			double score,
			DateTime submissionTime)
		{
			return new UserQuestionSubmission()
			{
				Score = score,
				DateSubmitted = submissionTime,
				UserQuestionData = new UserQuestionData()
				{
					User = user,
					UserId = user.Id,
					Question = question,
					QuestionId = question.Id,
				}
			};
		}

		/// <summary>
		/// Returns the expected section assignment results.
		/// </summary>
		private SectionAssignmentResults GetExpectedSectionAssignmentResults()
		{
			return new SectionAssignmentResults
			(
				"Unit 1",
				"Period1",
				4.0,
				new List<SectionAssignmentResult>()
				{
					new SectionAssignmentResult
					(
						"Unit 1",
						"Last1",
						"First1",
						3.7,
						new List<StudentQuestionResult>()
						{
							new StudentQuestionResult(0, "Question1", 1.0, 1.0, Status(Completion.Completed, late: false)),
							new StudentQuestionResult(0, "Question2", 1.0, 0.85, Status(Completion.Completed, late: true)),
							new StudentQuestionResult(0, "Question3", 1.0, 1.0, Status(Completion.Completed, late: false)),
							new StudentQuestionResult(0, "Question4", 1.0, 0.85, Status(Completion.Completed, late: true)),
						},
						new SubmissionStatus(Completion.Completed, late: true)
					),
					new SectionAssignmentResult
					(
						"Unit 1",
						"Last2",
						"First2",
						1.0,
						new List<StudentQuestionResult>()
						{
							new StudentQuestionResult(0, "Question1", 1.0, 1.0, Status(Completion.Completed, late: false)),
							new StudentQuestionResult(0, "Question2", 1.0, 0.0, Status(Completion.NotStarted, late: true)),
							new StudentQuestionResult(0, "Question3", 1.0, 0.0, Status(Completion.InProgress, late: true)),
							new StudentQuestionResult(0, "Question4", 1.0, 0.0, Status(Completion.NotStarted, late: true)),
						},
						new SubmissionStatus(Completion.InProgress, late: true)
					),
					new SectionAssignmentResult
					(
						"Unit 1",
						"Last3",
						"First3",
						1.0,
						new List<StudentQuestionResult>()
						{
							new StudentQuestionResult(0, "Question1", 1.0, 1.0, Status(Completion.Completed, late: false)),
							new StudentQuestionResult(0, "Question2", 1.0, 0.0, Status(Completion.NotStarted, late: true)),
							new StudentQuestionResult(0, "Question3", 1.0, 0.0, Status(Completion.NotStarted, late: true)),
							new StudentQuestionResult(0, "Question4", 1.0, 0.0, Status(Completion.NotStarted, late: true)),
						},
						new SubmissionStatus(Completion.InProgress, late: true)
					)
				}
			);
		}

		/// <summary>
		/// Returns the expected student assignment results.
		/// </summary>
		private StudentAssignmentResults GetExpectedStudentAssignmentResults()
		{
			return new StudentAssignmentResults
			(
				"Last1",
				"First1",
				"Period1",
				new List<StudentAssignmentResult>()
				{
					new StudentAssignmentResult
					(
						"Unit 1a",
						DueDate,
						2.85,
						new List<StudentQuestionResult>()
						{
							new StudentQuestionResult(0, "Question1", 1.0, 1.0, Status(Completion.Completed, late: false)),
							new StudentQuestionResult(0, "Question2", 1.0, 0.85, Status(Completion.Completed, late: true)),
							new StudentQuestionResult(0, "Question3", 1.0, 1.0, Status(Completion.Completed, late: false))
						},
						new SubmissionStatus(Completion.Completed, late: true)
					),
					new StudentAssignmentResult
					(
						"Unit 1b",
						DueDate,
						0.85,
						new List<StudentQuestionResult>()
						{
							new StudentQuestionResult(0, "Question4", 1.0, 0.85, Status(Completion.Completed, late: true)),
						},
						new SubmissionStatus(Completion.Completed, late: true)
					)
				}
			);
		}

		/// <summary>
		/// Returns the expected student assignment results.
		/// </summary>
		private UpdatedSectionAssignmentResults GetExpectedUpdatedAssignmentResults()
		{
			return new UpdatedSectionAssignmentResults
			(
				"Period1",
				"Gradebook1",
				DueDate,
				new List<SectionAssignmentResults>()
				{
					new SectionAssignmentResults
					(
						"Unit 1",
						"Period1",
						4.0,
						Collections.CreateList
						(
							new SectionAssignmentResult
							(
								"Unit 1",
								"Last1",
								"First1",
								3.7,
								new List<StudentQuestionResult>()
								{
									new StudentQuestionResult(0, "Question1", 1.0, 1.0, Status(Completion.Completed, late: false)),
									new StudentQuestionResult(0, "Question2", 1.0, 0.85, Status(Completion.Completed, late: true)),
									new StudentQuestionResult(0, "Question3", 1.0, 1.0, Status(Completion.Completed, late: false)),
									new StudentQuestionResult(0, "Question4", 1.0, 0.85, Status(Completion.Completed, late: true)),
								},
								new SubmissionStatus(Completion.Completed, late: true)
							)
						)
					)
				}
			);
		}

		/// <summary>
		/// Verifies updated results.
		/// </summary>
		private void VerifyUpdatedAssignmentResults(
			UpdatedSectionAssignmentResults expected,
			UpdatedSectionAssignmentResults actual)
		{
			Assert.Equal(expected.GradebookName, actual.GradebookName);
			Assert.Equal(expected.SectionName, actual.SectionName);
			Assert.Equal(expected.AssignmentsLastGradedDate, actual.AssignmentsLastGradedDate);
			Assert.Equal(expected.AssignmentResults.Count, actual.AssignmentResults.Count);
			for (int index = 0; index < expected.AssignmentResults.Count; index++)
			{
				VerifySectionAssignmentResults
				(
					expected.AssignmentResults[index],
					actual.AssignmentResults[index]
				);
			}
		}

		/// <summary>
		/// Verifies section results.
		/// </summary>
		private void VerifySectionAssignmentResults(
			SectionAssignmentResults expected,
			SectionAssignmentResults actual)
		{
			Assert.Equal(expected.AssignmentName, actual.AssignmentName);
			Assert.Equal(expected.SectionName, actual.SectionName);
			Assert.Equal(expected.Points, actual.Points);
			Assert.Equal(expected.AssignmentResults.Count, actual.AssignmentResults.Count);
			for (int index = 0; index < expected.AssignmentResults.Count; index++)
			{
				VerifySectionAssignmentResult
				(
					expected.AssignmentResults[index],
					actual.AssignmentResults[index]
				);
			}
		}

		/// <summary>
		/// Verifies assignment group results.
		/// </summary>
		private void VerifySectionAssignmentResult(
			SectionAssignmentResult expected,
			SectionAssignmentResult actual)
		{
			Assert.Equal(expected.AssignmentGroupName, actual.AssignmentGroupName);
			Assert.Equal(expected.LastName, actual.LastName);
			Assert.Equal(expected.FirstName, actual.FirstName);
			Assert.Equal(expected.Score, actual.Score);
			Assert.Equal(expected.Status.Completion, actual.Status.Completion);
			Assert.Equal(expected.Status.Late, actual.Status.Late);
			Assert.Equal(expected.QuestionResults.Count, actual.QuestionResults.Count);
			for (int index = 0; index < expected.QuestionResults.Count; index++)
			{
				VerifyStudentQuestionResult
				(
					expected.QuestionResults[index],
					actual.QuestionResults[index]
				);
			}
		}

		/// <summary>
		/// Verifies student results.
		/// </summary>
		private void VerifyStudentAssignmentResults(
			StudentAssignmentResults expected,
			StudentAssignmentResults actual)
		{
			Assert.Equal(expected.LastName, actual.LastName);
			Assert.Equal(expected.FirstName, actual.FirstName);
			Assert.Equal(expected.SectionName, actual.SectionName);
			Assert.Equal(expected.AssignmentResults.Count, actual.AssignmentResults.Count);
			for (int index = 0; index < expected.AssignmentResults.Count; index++)
			{
				VerifyStudentAssignmentResult
				(
					expected.AssignmentResults[index],
					actual.AssignmentResults[index]
				);
			}
		}

		/// <summary>
		/// Verifies student assignment results.
		/// </summary>
		private void VerifyStudentAssignmentResult(
			StudentAssignmentResult expected,
			StudentAssignmentResult actual)
		{
			Assert.Equal(expected.AssignmentName, actual.AssignmentName);
			Assert.Equal(expected.AssignmentDueDate, actual.AssignmentDueDate);
			Assert.Equal(expected.Score, actual.Score);
			Assert.Equal(expected.Status.Completion, actual.Status.Completion);
			Assert.Equal(expected.Status.Late, actual.Status.Late);
			Assert.Equal(expected.QuestionResults.Count, actual.QuestionResults.Count);
			for (int index = 0; index < expected.QuestionResults.Count; index++)
			{
				VerifyStudentQuestionResult
				(
					expected.QuestionResults[index],
					actual.QuestionResults[index]
				);
			}
		}

		/// <summary>
		/// Verifies a single question result.
		/// </summary>
		private void VerifyStudentQuestionResult(
			StudentQuestionResult expected,
			StudentQuestionResult actual)
		{
			Assert.Equal(expected.QuestionName, actual.QuestionName);
			Assert.Equal(expected.QuestionPoints, actual.QuestionPoints);
			Assert.Equal(expected.Score, actual.Score);
			Assert.Equal(expected.Status.Completion, actual.Status.Completion);
			Assert.Equal(expected.Status.Late, actual.Status.Late);
		}

		/// <summary>
		/// Returns a new SubmissionStatus object.
		/// </summary>
		private SubmissionStatus Status(Completion completion, bool late)
		{
			return new SubmissionStatus(completion, late);
		}
	}
}