using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Questions.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the QuestionResultGenerator class.
	/// </summary>
	public class QuestionResultGenerator_UnitTests
	{
		/// <summary>
		/// An example due date.
		/// </summary>
		private readonly DateTime DueDate = new DateTime(2017, 1, 1, 0, 0, 0);

		/// <summary>
		/// Verifies that CreateQuestionResult returns a StudentQuestionResult
		/// object with the correct simple properties.
		/// </summary>
		[Fact]
		public void CreateQuestionResult_ReturnsCorrectSimpleProperties()
		{
			var assignmentQuestion = CreateAssignmentQuestion();
			var user = CreateUser();

			var questionResultGenerator = CreateQuestionResultGenerator
			(
				assignmentQuestion
			);

			var result = questionResultGenerator.CreateQuestionResult
			(
				assignmentQuestion,
				user,
				new List<UserQuestionSubmission>(),
				DueDate
			);

			Assert.Equal(assignmentQuestion.Id, result.QuestionId);
			Assert.Equal(assignmentQuestion.AssignmentId, result.AssignmentId);
			Assert.Equal(user.Id, result.UserId);
			Assert.Equal(assignmentQuestion.Assignment.CombinedSubmissions, result.CombinedSubmissions);
			Assert.Equal(assignmentQuestion.Name, result.QuestionName);
			Assert.Equal(assignmentQuestion.Points, result.QuestionPoints);
		}

		/// <summary>
		/// Ensures that the score on the returned StudentQuestionResult object has the
		/// score and status of the highest scoring submission.
		/// </summary>
		[Fact]
		public void CreateQuestionResult_ReturnsScoreAndStatusOfHighestScoringSubmission()
		{
			var assignmentQuestion = CreateAssignmentQuestion();
			var user = CreateUser();
			var submissions = CreateScoredSubmissions();

			var questionResultGenerator = CreateQuestionResultGenerator
			(
				assignmentQuestion,
				submissions
			);

			var result = questionResultGenerator.CreateQuestionResult
			(
				assignmentQuestion,
				user,
				submissions.Select(s => s.Submission).ToList(),
				DueDate
			);

			Assert.Equal(0.9, result.Score);
			Assert.Equal(Completion.Completed, result.Status.Completion);
			Assert.True(result.Status.Late);
		}

		/// <summary>
		/// Ensures that a question supporting interactive submissions in an assignment
		/// without combined submissions does not return submission results for past 
		/// submissions. (Questions supporting interactive submissions do not show
		/// answers or explanations.)
		/// </summary>
		[Fact]
		public void CreateQuestionResult_SeparateInteractiveSubmissions_NoSubmissionResults()
		{
			var assignmentQuestion = CreateAssignmentQuestion(
				supportsInteractiveSubmissions: true,
				combinedSubmissions: false);

			var user = CreateUser();
			var submissions = CreateScoredSubmissions();

			var questionResultGenerator = CreateQuestionResultGenerator
			(
				assignmentQuestion,
				submissions
			);

			var result = questionResultGenerator.CreateQuestionResult
			(
				assignmentQuestion,
				user,
				submissions.Select(s => s.Submission).ToList(),
				DueDate
			);

			Assert.Null(result.SubmissionResults);
		}

		/// <summary>
		/// Ensures that a question in an assignment with combined submissions does not 
		/// return submission results for past question submissions. (Assignments with 
		/// combined submissions group all past question submissions at a higher level,
		/// under an AssignmentSubmissionResult.)
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void CreateQuestionResult_CombinedSubmissions_NoSubmissionResults(
			bool supportsInteractiveSubmissions)
		{
			var assignmentQuestion = CreateAssignmentQuestion(
				supportsInteractiveSubmissions,
				combinedSubmissions: true);

			var user = CreateUser();
			var submissions = CreateScoredSubmissions();

			var questionResultGenerator = CreateQuestionResultGenerator
			(
				assignmentQuestion,
				submissions
			);

			var result = questionResultGenerator.CreateQuestionResult
			(
				assignmentQuestion,
				user,
				submissions.Select(s => s.Submission).ToList(),
				DueDate
			);

			Assert.Null(result.SubmissionResults);
		}

		/// <summary>
		/// Ensures that a question that does not support interactive submissions, in an 
		/// assignment without combined submissions, returns all past question submission
		/// results.
		/// </summary>
		[Fact]
		public void CreateQuestionResult_SeparateNonInteractiveSubmissions_SubmissionResultsOrderedByDate()
		{
			var assignmentQuestion = CreateAssignmentQuestion(
				supportsInteractiveSubmissions: false,
				combinedSubmissions: false);

			var user = CreateUser();
			var submissions = CreateScoredSubmissions();

			var questionResultGenerator = CreateQuestionResultGenerator
			(
				assignmentQuestion,
				submissions
			);

			var result = questionResultGenerator.CreateQuestionResult
			(
				assignmentQuestion,
				user,
				submissions.Select(s => s.Submission).ToList(),
				DueDate
			);

			var orderedSubmissions = submissions
				.OrderBy(s => s.Submission.DateSubmitted)
				.ToList();

			Assert.Equal(orderedSubmissions.Count, result.SubmissionResults.Count);
			for (int i = 0; i < orderedSubmissions.Count; i++)
			{
				var submissionResult = result.SubmissionResults[i];

				Assert.Equal(assignmentQuestion.Id, submissionResult.QuestionId);
				Assert.Equal(assignmentQuestion.AssignmentId, submissionResult.AssignmentId);
				Assert.Equal(user.Id, submissionResult.UserId);
				Assert.Equal(orderedSubmissions[i].Submission.DateSubmitted, submissionResult.SubmissionDate);
				Assert.Equal(orderedSubmissions[i].Status, submissionResult.Status);
				Assert.Equal(orderedSubmissions[i].Score, submissionResult.Score);
				Assert.Equal(assignmentQuestion.Points, submissionResult.QuestionPoints);
			}
		}

		/// <summary>
		/// Creates an assignment question.
		/// </summary>
		private AssignmentQuestion CreateAssignmentQuestion(
			bool supportsInteractiveSubmissions = true, 
			bool combinedSubmissions = false)
		{
			return new AssignmentQuestion()
			{
				Id = 1,
				AssignmentId = 2,
				Assignment = new Assignment()
				{
					Id = 2,
					CombinedSubmissions = combinedSubmissions
				},
				Name = "Question Name",
				Points = 3,
				Question = supportsInteractiveSubmissions
					? (Question) new MultipleChoiceQuestion()
					: (Question) new GeneratedQuestionTemplate()
			};
		}

		/// <summary>
		/// Creates a new user.
		/// </summary>
		private User CreateUser()
		{
			return new User() { Id = 10 };
		}

		/// <summary>
		/// Returns a list of question submissions.
		/// </summary>
		private IList<ScoredSubmission> CreateScoredSubmissions()
		{
			return Collections.CreateList
			(
				CreateScoredSubmission(daysLate: 0, score: 0.5, isLate: false),
				CreateScoredSubmission(daysLate: 2, score: 0.8, isLate: true),
				CreateScoredSubmission(daysLate: 1, score: 0.9, isLate: true)
			);
		}

		/// <summary>
		/// Returns a user question submission with a score.
		/// </summary>
		private ScoredSubmission CreateScoredSubmission(
			int daysLate,
			double score,
			bool isLate)
		{
			return new ScoredSubmission
			(
				new UserQuestionSubmission()
				{
					DateSubmitted = DueDate + TimeSpan.FromDays(daysLate)
				},
				score,
				isLate
			);
		}

		/// <summary>
		/// Creates a question score calculator.
		/// </summary>
		private IQuestionScoreCalculator CreateMockQuestionScoreCalculator(
			AssignmentQuestion assignmentQuestion,
			IList<ScoredSubmission> scoredSubmissions)
		{
			var questionScoreCalculator = new Mock<IQuestionScoreCalculator>();

			if (scoredSubmissions != null)
			{
				foreach (var scoredSubmission in scoredSubmissions)
				{
					questionScoreCalculator
						.Setup
						(
							m => m.GetSubmissionScore
							(
								scoredSubmission.Submission,
								DueDate,
								assignmentQuestion.Points,
								true /*withLateness*/
							)
						).Returns(scoredSubmission.Score);
				}
			}

			return questionScoreCalculator.Object;
		}

		/// <summary>
		/// Creates a submission status calculator.
		/// </summary>
		private ISubmissionStatusCalculator CreateMockSubmissionStatusCalculator(
			AssignmentQuestion assignmentQuestion,
			IList<ScoredSubmission> scoredSubmissions = null)
		{
			var submissionStatusCalculator = new Mock<ISubmissionStatusCalculator>();

			if ((scoredSubmissions?.Count ?? 0) == 0)
			{
				submissionStatusCalculator
					.Setup
					(
						m => m.GetStatusForQuestion
						(
							null /*dateSubmitted*/,
							DueDate,
							assignmentQuestion.IsInteractive(),
							0.0 /*score*/
						)
					).Returns(new SubmissionStatus(Completion.NotStarted, late: false));
			}
			else
			{
				foreach (var scoredSubmission in scoredSubmissions)
				{
					submissionStatusCalculator
						.Setup
						(
							m => m.GetStatusForQuestion
							(
								scoredSubmission.Submission.DateSubmitted,
								DueDate,
								assignmentQuestion.IsInteractive(),
								scoredSubmission.Score
							)
						).Returns(scoredSubmission.Status);
				}
			}

			return submissionStatusCalculator.Object;
		}

		/// <summary>
		/// Creates a new question result generator.
		/// </summary>
		private IQuestionResultGenerator CreateQuestionResultGenerator(
			AssignmentQuestion assignmentQuestion,
			IList<ScoredSubmission> submissions = null)
		{
			var submissionStatusCalculator = CreateMockSubmissionStatusCalculator
			(
				assignmentQuestion, 
				submissions
			);

			var questionScoreCalculator = CreateMockQuestionScoreCalculator
			(
				assignmentQuestion,
				submissions
			);

			return new QuestionResultGenerator
			(
				questionScoreCalculator, 
				submissionStatusCalculator
			);
		}
	}
}
