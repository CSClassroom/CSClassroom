using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using MoreLinq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the AssignmentResultGenerator class
	/// </summary>
	public class AssignmentResultGenerator_UnitTests
	{
		/// <summary>
		/// Example dates.
		/// </summary>
		private readonly DateTime DueDate = new DateTime(2017, 1, 1, 0, 0, 0);
		private readonly DateTime SubmissionDate1 = new DateTime(2017, 1, 2, 0, 0, 0);
		private readonly DateTime SubmissionDate2 = new DateTime(2017, 1, 3, 0, 0, 0);

		/// <summary>
		/// Verifies that CreateAssignmentResult returns an AssignmentResult
		/// object with the correct simple properties.
		/// </summary>
		[Fact]
		public void CreateAssignmentResult_ReturnsCorrectSimpleProperties()
		{
			var section = new Section();
			var user = new User();
			var assignment = CreateAssignment(section);
			var submissions = CreateSubmissions(assignment);

			var assignmentResultGenerator = CreateAssignmentResultGenerator
			(
				user,
				assignment,
				submissions
			);

			var result = assignmentResultGenerator.CreateAssignmentResult
			(
				section,
				assignment,
				user,
				false /*admin*/,
				submissions.Select(s => s.Submission).ToList()
			);

			Assert.Equal(assignment.Name, result.AssignmentName);
			Assert.Equal(assignment.Id, result.AssignmentId);
			Assert.Equal(user.Id, result.UserId);
			Assert.Equal(DueDate, result.AssignmentDueDate);
			Assert.Equal(assignment.Questions.Sum(q => q.Points), result.TotalPoints);
		}

		/// <summary>
		/// Verifies that CreateAssignmentResult returns the correct score and status
		/// when the assignment does not have combined submissions.
		/// </summary>
		[Fact]
		public void CreateAssignmentResult_SeparateSubmissions_ReturnsCorrectScoreAndStatus()
		{
			var section = new Section();
			var user = new User();
			var assignment = CreateAssignment(section, combinedSubmissions: false);
			var submissions = CreateSubmissions(assignment);

			var assignmentResultGenerator = CreateAssignmentResultGenerator
			(
				user,
				assignment,
				submissions
			);

			var result = assignmentResultGenerator.CreateAssignmentResult
			(
				section,
				assignment,
				user,
				false /*admin*/,
				submissions.Select(s => s.Submission).ToList()
			);

			Assert.Equal(9.0, result.Score);
			Assert.Equal(Completion.Completed, result.Status.Completion);
			Assert.True(result.Status.Late);
		}

		/// <summary>
		/// Verifies that CreateAssignmentResult returns the correct score and status
		/// when the assignment does have combined submissions.
		/// </summary>
		[Fact]
		public void CreateAssignmentResult_CombinedSubmissions_ReturnsCorrectScoreAndStatus()
		{
			var section = new Section();
			var user = new User();
			var assignment = CreateAssignment(section, combinedSubmissions: true);
			var submissions = CreateSubmissions(assignment);

			var assignmentResultGenerator = CreateAssignmentResultGenerator
			(
				user,
				assignment,
				submissions
			);

			var result = assignmentResultGenerator.CreateAssignmentResult
			(
				section,
				assignment,
				user,
				false /*admin*/,
				submissions.Select(s => s.Submission).ToList()
			);

			Assert.Equal(7.0, result.Score);
			Assert.Equal(Completion.Completed, result.Status.Completion);
			Assert.True(result.Status.Late);
		}

		/// <summary>
		/// Verifies that CreateAssignmentResult returns question results when appropriate,
		/// when the assignment does not have combined submissions.
		/// </summary>
		[Theory]
		[InlineData(true /*onlyShowCombinedScore*/, false /*admin*/, false /*expectedQuestionResults*/)]
		[InlineData(false /*onlyShowCombinedScore*/, false /*admin*/, true /*expectedQuestionResults*/)]
		[InlineData(false /*onlyShowCombinedScore*/, true /*admin*/, true /*expectedQuestionResults*/)]
		public void CreateAssignmentResult_SeparateSubmissions_CorrectQuestionResultsWhenAppropriate(
			bool onlyShowCombinedScore,
			bool admin,
			bool expectedQuestionResults)
		{
			var section = new Section();
			var user = new User();
			var assignment = CreateAssignment(section, false /*combinedSubmissions*/, onlyShowCombinedScore);
			var submissions = CreateSubmissions(assignment);

			var assignmentResultGenerator = CreateAssignmentResultGenerator
			(
				user,
				assignment,
				submissions
			);

			var result = (SeparateSubmissionsAssignmentResult)assignmentResultGenerator.CreateAssignmentResult
			(
				section,
				assignment,
				user,
				admin,
				submissions.Select(s => s.Submission).ToList()
			);

			if (expectedQuestionResults)
			{
				Assert.Equal(2, result.QuestionResults.Count);

				{
					var questionResult = result.QuestionResults[0];

					Assert.Equal(assignment.Questions[0].Id, questionResult.QuestionId);
					Assert.Equal(4.0, questionResult.Score);
					Assert.Equal(Completion.Completed, questionResult.Status.Completion);
					Assert.True(questionResult.Status.Late);
				}

				{
					var questionResult = result.QuestionResults[1];

					Assert.Equal(assignment.Questions[1].Id, questionResult.QuestionId);
					Assert.Equal(5.0, questionResult.Score);
					Assert.Equal(Completion.Completed, questionResult.Status.Completion);
					Assert.False(questionResult.Status.Late);
				}
			}
			else
			{
				Assert.Equal(0, result.QuestionResults.Count);
			}
		}

		/// <summary>
		/// Verifies that CreateAssignmentResult returns question results when appropriate,
		/// when the assignment does have combined submissions.
		/// </summary>
		[Theory]
		[InlineData(true /*onlyShowCombinedScore*/, false /*admin*/, false /*expectedQuestionResults*/)]
		[InlineData(false /*onlyShowCombinedScore*/, false /*admin*/, true /*expectedQuestionResults*/)]
		[InlineData(false /*onlyShowCombinedScore*/, true /*admin*/, true /*expectedQuestionResults*/)]
		public void CreateAssignmentResult_CombinedSubmissions_CorrectQuestionResultsWhenAppropriate(
			bool onlyShowCombinedScore,
			bool admin,
			bool expectedQuestionResults)
		{
			var section = new Section();
			var user = new User();
			var assignment = CreateAssignment(section, true /*combinedSubmissions*/, onlyShowCombinedScore);
			var submissions = CreateSubmissions(assignment);

			var assignmentResultGenerator = CreateAssignmentResultGenerator
			(
				user,
				assignment,
				submissions
			);

			var result = (CombinedSubmissionsAssignmentResult)assignmentResultGenerator.CreateAssignmentResult
			(
				section,
				assignment,
				user,
				admin,
				submissions.Select(s => s.Submission).ToList()
			);

			Assert.Equal(2, result.AssignmentSubmissionResults.Count);

			{
				var assignmentSubmissionResult = result.AssignmentSubmissionResults[0];

				Assert.Equal(user.Id, assignmentSubmissionResult.UserId);
				Assert.Equal(SubmissionDate1, assignmentSubmissionResult.SubmissionDate);
				Assert.Equal(6.0, assignmentSubmissionResult.Score);
				Assert.Equal(Completion.Completed, assignmentSubmissionResult.Status.Completion);
				Assert.False(assignmentSubmissionResult.Status.Late);
				Assert.Equal(11.0, assignmentSubmissionResult.AssignmentPoints);

				if (expectedQuestionResults)
				{
					{
						var questionResult = assignmentSubmissionResult.QuestionResults[0];

						Assert.Equal(assignment.Questions[0].Id, questionResult.QuestionId);
						Assert.Equal(1.0, questionResult.Score);
						Assert.Equal(Completion.Completed, questionResult.Status.Completion);
						Assert.False(questionResult.Status.Late);
					}

					{
						var questionResult = assignmentSubmissionResult.QuestionResults[1];

						Assert.Equal(assignment.Questions[1].Id, questionResult.QuestionId);
						Assert.Equal(5.0, questionResult.Score);
						Assert.Equal(Completion.Completed, questionResult.Status.Completion);
						Assert.False(questionResult.Status.Late);
					}
				}
				else
				{
					Assert.Equal(0, assignmentSubmissionResult.QuestionResults.Count);
				}
			}

			{
				var assignmentSubmissionResult = result.AssignmentSubmissionResults[1];

				Assert.Equal(user.Id, assignmentSubmissionResult.UserId);
				Assert.Equal(SubmissionDate2, assignmentSubmissionResult.SubmissionDate);
				Assert.Equal(7.0, assignmentSubmissionResult.Score);
				Assert.Equal(Completion.Completed, assignmentSubmissionResult.Status.Completion);
				Assert.True(assignmentSubmissionResult.Status.Late);
				Assert.Equal(11.0, assignmentSubmissionResult.AssignmentPoints);

				if (expectedQuestionResults)
				{
					{
						var questionResult = assignmentSubmissionResult.QuestionResults[0];

						Assert.Equal(assignment.Questions[0].Id, questionResult.QuestionId);
						Assert.Equal(4.0, questionResult.Score);
						Assert.Equal(Completion.Completed, questionResult.Status.Completion);
						Assert.True(questionResult.Status.Late);
					}

					{
						var questionResult = assignmentSubmissionResult.QuestionResults[1];

						Assert.Equal(assignment.Questions[1].Id, questionResult.QuestionId);
						Assert.Equal(3.0, questionResult.Score);
						Assert.Equal(Completion.Completed, questionResult.Status.Completion);
						Assert.True(questionResult.Status.Late);
					}
				}
				else
				{
					Assert.Equal(0, assignmentSubmissionResult.QuestionResults.Count);
				}
			}
		}

		/// <summary>
		/// Creates an assignment with two questions.
		/// </summary>
		private Assignment CreateAssignment(
			Section section,
			bool combinedSubmissions = false,
			bool onlyShowCombinedScore = false)
		{
			var assignment = new Assignment()
			{
				Id = 1,
				Name = "Assignment Name",
				CombinedSubmissions = combinedSubmissions,
				OnlyShowCombinedScore = onlyShowCombinedScore,
				DueDates = Collections.CreateList
				(
					new AssignmentDueDate()
					{
						Section = section,
						DueDate = DueDate
					}
				),
				Questions = Collections.CreateList
				(
					new AssignmentQuestion()
					{
						Id = 10,
						Name = "Question 1",
						Order = 0,
						Points = 5.0
					},
					new AssignmentQuestion()
					{
						Id = 20,
						Name = "Question 2",
						Order = 1,
						Points = 6.0
					}
				)
			};

			foreach (var question in assignment.Questions)
			{
				question.Assignment = assignment;
				question.AssignmentId = assignment.Id;
			}

			return assignment;
		}

		/// <summary>
		/// Returns a list of submissions for an assignment
		/// (two submissions per question).
		/// </summary>
		private IList<ScoredSubmission> CreateSubmissions(Assignment assignment)
		{
			return Collections.CreateList
			(
				CreateSubmission(assignment.Questions[0], SubmissionDate1, 1.0, isLate: false),
				CreateSubmission(assignment.Questions[0], SubmissionDate2, 4.0, isLate: true),
				CreateSubmission(assignment.Questions[1], SubmissionDate1, 5.0, isLate: false),
				CreateSubmission(assignment.Questions[1], SubmissionDate2, 3.0, isLate: true)
			);
		}

		/// <summary>
		/// Creates a new submission.
		/// </summary>
		private ScoredSubmission CreateSubmission(
			AssignmentQuestion assignmentQuestion,
			DateTime dateSubmitted,
			double score,
			bool isLate)
		{
			return new ScoredSubmission
			(
				new UserQuestionSubmission()
				{
					DateSubmitted = dateSubmitted,
					UserQuestionData = new UserQuestionData()
					{
						AssignmentQuestion = assignmentQuestion
					}
				},
				score,
				isLate
			);
		}

		/// <summary>
		/// Creates a question result generator.
		/// </summary>
		private void SetupMocks(
			Mock<IQuestionResultGenerator> generator,
			Mock<IAssignmentScoreCalculator> calculator,
			User user,
			Assignment assignment,
			IList<ScoredSubmission> submissions)
		{
			if (assignment.CombinedSubmissions)
			{
				SetupCreateQuestionResult(generator, user, assignment.Questions[0], submissions[0]);
				SetupCreateQuestionResult(generator, user, assignment.Questions[0], submissions[1]);
				SetupCreateQuestionResult(generator, user, assignment.Questions[1], submissions[2]);
				SetupCreateQuestionResult(generator, user, assignment.Questions[1], submissions[3]);

				SetupGetAssignmentScoreFromAssignmentSubmissionResults(calculator, 6.0, 7.0);

				SetupGetAssignmentStatusFromQuestionResults(calculator, GetScoreAndStatus(1.0, false), GetScoreAndStatus(5.0, false));
				SetupGetAssignmentStatusFromQuestionResults(calculator, GetScoreAndStatus(4.0, true), GetScoreAndStatus(3.0, true));
				SetupGetAssignmentStatusFromAssignmentSubmissionResults(calculator, GetScoreAndStatus(6.0, false), GetScoreAndStatus(7.0, true));
			}
			else
			{
				SetupCreateQuestionResult(generator, user, assignment.Questions[0], submissions[0], submissions[1]);
				SetupCreateQuestionResult(generator, user, assignment.Questions[1], submissions[2], submissions[3]);

				SetupGetAssignmentScoreFromQuestionResults(calculator, 4.0, 5.0);

				SetupGetAssignmentStatusFromQuestionResults(calculator, GetScoreAndStatus(4.0, true), GetScoreAndStatus(5.0, false));
			}
		}

		/// <summary>
		/// Setup the CreateQuestionResult method on the mock question result generator.
		/// </summary>
		private void SetupCreateQuestionResult(
			Mock<IQuestionResultGenerator> questionResultGenerator,
			User user,
			AssignmentQuestion assignmentQuestion,
			params ScoredSubmission[] submissions)
		{
			questionResultGenerator
				.Setup
				(
					m => m.CreateQuestionResult
					(
						assignmentQuestion,
						user,
						It.Is<IList<UserQuestionSubmission>>
						(
							seq => seq.SequenceEqual(submissions.Select(s => s.Submission))
						),
						DueDate
					)
				).Returns(CreateQuestionResult(user, assignmentQuestion, submissions));
		}

		/// <summary>
		/// Sets up the GetAssignmentScore method on the mock assignment score calculator. 
		/// </summary>
		private void SetupGetAssignmentScoreFromQuestionResults(
			Mock<IAssignmentScoreCalculator> calculator,
			params double[] questionResultScores)
		{
			calculator
				.Setup
				(
					m => m.GetAssignmentScore
					(
						It.Is<IList<StudentQuestionResult>>
						(
							seq => seq.Select(s => s.Score).SequenceEqual
							(
								questionResultScores
							)
						),
						2 /*roundDigits*/
					)
				).Returns(questionResultScores.Sum());
		}

		/// <summary>
		/// Sets up the GetAssignmentScore method on the mock assignment score calculator. 
		/// </summary>
		private void SetupGetAssignmentScoreFromAssignmentSubmissionResults(
			Mock<IAssignmentScoreCalculator> calculator,
			params double[] assignmentSubmissionScores)
		{
			calculator
				.Setup
				(
					m => m.GetAssignmentScore
					(
						It.Is<IList<AssignmentSubmissionResult>>
						(
							seq => seq.Select(s => s.Score).SequenceEqual
							(
								assignmentSubmissionScores
							)
						),
						2 /*roundDigits*/
					)
				).Returns(assignmentSubmissionScores.Max());
		}

		/// <summary>
		/// Sets up the GetAssignmentStatus method on the mock assignment score calculator. 
		/// </summary>
		private void SetupGetAssignmentStatusFromQuestionResults(
			Mock<IAssignmentScoreCalculator> calculator,
			params Tuple<double, bool>[] scoresAndStatus)
		{
			calculator
				.Setup
				(
					m => m.GetAssignmentStatus
					(
						It.Is<IList<StudentQuestionResult>>
						(
							seq => seq.Select(GetScoreAndStatus).SequenceEqual(scoresAndStatus)
						)
					)
				)
				.Returns
				(
					new SubmissionStatus
					(
						Completion.Completed, scoresAndStatus.Any(r => r.Item2)
					)
				);
		}

		/// <summary>
		/// Sets up the GetAssignmentStatus method on the mock assignment score calculator. 
		/// </summary>
		private void SetupGetAssignmentStatusFromAssignmentSubmissionResults(
			Mock<IAssignmentScoreCalculator> calculator,
			params Tuple<double, bool>[] scoresAndStatus)
		{
			calculator
				.Setup
				(
					m => m.GetAssignmentStatus
					(
						It.Is<IList<AssignmentSubmissionResult>>
						(
							seq => seq.Select(GetScoreAndStatus).SequenceEqual(scoresAndStatus)
						),
						DueDate
					)
				)
				.Returns
				(
					new SubmissionStatus
					(
						Completion.Completed, 
						scoresAndStatus.MaxBy(s => s.Item1).Item2
					)
				);
		}

		/// <summary>
		/// Returns the score and status of an assignment submission result.
		/// </summary>
		private Tuple<double, bool> GetScoreAndStatus(double score, bool isLate)
		{
			return new Tuple<double, bool>(score, isLate);
		}

		/// <summary>
		/// Returns the score and status of an assignment submission result.
		/// </summary>
		private Tuple<double, bool> GetScoreAndStatus(AssignmentSubmissionResult result)
		{
			return new Tuple<double, bool>(result.Score, result.Status.Late);
		}

		/// <summary>
		/// Returns the score and status of an assignment submission result.
		/// </summary>
		private Tuple<double, bool> GetScoreAndStatus(StudentQuestionResult result)
		{
			return new Tuple<double, bool>(result.Score, result.Status.Late);
		}

		/// <summary>
		/// Creates a question result.
		/// </summary>
		private StudentQuestionResult CreateQuestionResult(
			User user,
			AssignmentQuestion assignmentQuestion,
			params ScoredSubmission[] submissions)
		{
			return new StudentQuestionResult
			(
				assignmentQuestion.Id,
				0 /*assignmentId*/,
				0 /*userId*/,
				false /*combinedSubmissions*/,
				"" /*name*/,
				0 /*points*/,
				submissions.Max(s => s.Score),
				new SubmissionStatus
				(
					Completion.Completed, 
					submissions.MaxBy(s => s.Score).Status.Late
				),
				submissionResults: null
			);
		}

		/// <summary>
		/// Creates an assignment result generator.
		/// </summary>
		private AssignmentResultGenerator CreateAssignmentResultGenerator(
			User user,
			Assignment assignment,
			IList<ScoredSubmission> submissions)
		{
			var questionResultGenerator = new Mock<IQuestionResultGenerator>();
			var assignmentScoreCalculator = new Mock<IAssignmentScoreCalculator>();

			SetupMocks
			(
				questionResultGenerator,
				assignmentScoreCalculator,
				user,
				assignment,
				submissions
			);

			return new AssignmentResultGenerator
			(
				questionResultGenerator.Object,
				assignmentScoreCalculator.Object
			);
		}
	}
}