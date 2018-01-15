using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.Assignments.QuestionGraders;
using CSC.CSClassroom.Service.Assignments.QuestionResolvers;
using CSC.CSClassroom.Service.Assignments.QuestionSolvers;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments
{
	/// <summary>
	/// Unit tests for the AssignmentService class.
	/// </summary>
	public class AssignmentQuestionService_UnitTests
	{
		/// <summary>
		/// An example submission date.
		/// </summary>
		private readonly DateTime SubmissionDate = new DateTime(2017, 1, 1, 0, 0, 0);

		/// <summary>
		/// An example due date.
		/// </summary>
		private readonly DateTime DueDate = new DateTime(2017, 1, 2, 0, 0, 0);

		/// <summary>
		/// Ensures that GetAssignmentQuestionsAsync returns the set of
		/// questions for the assignment.
		/// </summary>
		[Fact]
		public async Task GetAssignmentQuestionsAsync_ReturnsQuestions()
		{
			var database = GetDatabase().Build();

			var assignmentId = database.Context
				.Assignments
				.Single(a => a.Name == "Unit 1a")
				.Id;

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				database.Context
			);

			var results = await assignmentQuestionService.GetAssignmentQuestionsAsync
			(
				"Class1",
				assignmentId
			);

			Assert.Equal(1, results.Count);
			Assert.Equal("Question1", results[0].Name);
		}

		/// <summary>
		/// Ensures that GetQuestionToSolveAsync returns the correct result.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_NoUnsolvedPrereqs_ReturnsResult()
		{
			var database = GetDatabase().Build();

			var dataLoaderFactory = GetMockUserQuestionDataLoaderFactory
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				assignmentQuestionIds: new[] { 3 }
			);

			var questionSolver = GetMockQuestionSolver
			(
				Collections.CreateList
				(
					CreateQuestionToSolve(assignmentQuestionId: 3)
				)
			);

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context,
				userQuestionDataLoaderFactory: dataLoaderFactory,
				questionSolver: questionSolver
			);

			var result = await assignmentQuestionService.GetQuestionToSolveAsync
			(
				"Class1",
				assignmentId: 1,
				assignmentQuestionId: 3,
				userId: 2
			);

			Assert.Equal(3, result.AssignmentQuestionId);
		}

		/// <summary>
		/// Ensures that GetQuestionToSolveAsync returns the correct result.
		/// </summary>
		[Fact]
		public async Task GetQuestionsToSolveAsync_ReturnsResults()
		{
			var database = GetDatabase().Build();

			var dataLoaderFactory = GetMockUserQuestionDataLoaderFactory
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				assignmentQuestionIds: new[] { 3, 4, 5 }
			);

			var questionsToSolve = Collections.CreateList
			(
				CreateQuestionToSolve(assignmentQuestionId: 3, remaining: 1),
				CreateQuestionToSolve(assignmentQuestionId: 4, remaining: 0),
				CreateQuestionToSolve(assignmentQuestionId: 5, remaining: 1)
			);

			var questionSolver = GetMockQuestionSolver(questionsToSolve);

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context,
				userQuestionDataLoaderFactory: dataLoaderFactory,
				questionSolver: questionSolver
			);

			var results = await assignmentQuestionService.GetQuestionsToSolveAsync
			(
				"Class1",
				assignmentId: 1,
				userId: 2
			);

			Assert.Equal(2, results.Count);
			Assert.Equal(questionsToSolve[0], results[0]);
			Assert.Equal(questionsToSolve[2], results[1]);
		}

		/// <summary>
		/// Ensures that GradeSubmissionAsync returns the correct result.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ReturnsResult()
		{
			var database = GetDatabase().Build();

			var submission = CreateQuestionSubmission(assignmentQuestionId: 3);
			var timeProvider = GetMockTimeProvider(SubmissionDate);
			var dataLoaderFactory = GetMockUserQuestionDataLoaderFactory
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				assignmentQuestionIds: new[] { 3 }
			);

			var questionSolver = GetMockQuestionSolver
			(
				submissionsToGrade: Collections.CreateList(submission),
				dateSubmitted: SubmissionDate
			);

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context,
				userQuestionDataLoaderFactory: dataLoaderFactory,
				questionSolver: questionSolver,
				timeProvider: timeProvider
			);

			var result = await assignmentQuestionService.GradeSubmissionAsync
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				submission: submission
			);

			Assert.NotNull(result);
		}

		/// <summary>
		/// Ensures that GradeSubmissionsAsync returns the correct results.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionsAsync_ReturnsResults()
		{
			var database = GetDatabase().Build();

			var submissions = Collections.CreateList
			(
				CreateQuestionSubmission(assignmentQuestionId: 3),
				CreateQuestionSubmission(assignmentQuestionId: 4),
				CreateQuestionSubmission(assignmentQuestionId: 5)
			);

			var timeProvider = GetMockTimeProvider(SubmissionDate);

			var dataLoaderFactory = GetMockUserQuestionDataLoaderFactory
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				assignmentQuestionIds: new[] { 3, 4, 5 }
			);

			var questionSolver = GetMockQuestionSolver
			(
				submissionsToGrade: submissions,
				dateSubmitted: SubmissionDate
			);

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context,
				userQuestionDataLoaderFactory: dataLoaderFactory,
				questionSolver: questionSolver,
				timeProvider: timeProvider
			);

			var result = await assignmentQuestionService.GradeSubmissionsAsync
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				submissions: submissions
			);

			Assert.Equal(SubmissionDate, result.SubmissionDate);
		}

		/// <summary>
		/// Ensures that GetSubmissionAsync returns the correct result.
		/// </summary>
		[Theory]
		[InlineData(true /*hasDueDate*/)]
		[InlineData(false /*hasDueDate*/)]
		public async Task GetSubmissionAsync_ReturnsResult(bool hasDueDate)
		{
			var database = GetDatabase().Build();

			var submissionResult = CreateSubmissionResult(assignmentQuestionId: 3);
			var timeProvider = GetMockTimeProvider(SubmissionDate);
			var dataLoaderFactory = GetMockUserQuestionDataLoaderFactory
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				assignmentQuestionIds: new[] { 3 }
			);

			var questionSolver = GetMockQuestionSolver
			(
				submissionResults: Collections.CreateList(submissionResult),
				dateSubmitted: SubmissionDate,
				dateDue: hasDueDate ? DueDate : (DateTime?)null
			);

			var assignmentDueDateRetriever = GetMockAssignmentDueDateRetriever
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				dueDate: hasDueDate
					? DueDate
					: (DateTime?)null
			);

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context,
				userQuestionDataLoaderFactory: dataLoaderFactory,
				assignmentDueDateRetriever: assignmentDueDateRetriever,
				questionSolver: questionSolver,
				timeProvider: timeProvider
			);

			var result = await assignmentQuestionService.GetSubmissionAsync
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				assignmentQuestionId: 3,
				submissionDate: SubmissionDate
			);

			Assert.Equal(submissionResult, result);
		}

		/// <summary>
		/// Ensures that GetSubmissionsAsync returns the correct results.
		/// </summary>
		[Theory]
		[InlineData(true /*hasDueDate*/)]
		[InlineData(false /*hasDueDate*/)]
		public async Task GetSubmissionsAsync_ReturnsResult(bool hasDueDate)
		{
			var database = GetDatabase().Build();
			var timeProvider = GetMockTimeProvider(SubmissionDate);
			var submissionResults = Collections.CreateList
			(
				CreateSubmissionResult(assignmentQuestionId: 3),
				CreateSubmissionResult(assignmentQuestionId: 4),
				CreateSubmissionResult(assignmentQuestionId: 5)
			);

			var dataLoaderFactory = GetMockUserQuestionDataLoaderFactory
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				assignmentQuestionIds: new[] { 3, 4, 5 }
			);

			var assignmentDueDateRetriever = GetMockAssignmentDueDateRetriever
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				dueDate: hasDueDate
					? DueDate
					: (DateTime?) null
			);

			var questionSolver = GetMockQuestionSolver
			(
				submissionResults: submissionResults,
				dateSubmitted: SubmissionDate,
				dateDue: hasDueDate ? DueDate : (DateTime?)null
			);

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context,
				userQuestionDataLoaderFactory: dataLoaderFactory,
				assignmentDueDateRetriever: assignmentDueDateRetriever,
				questionSolver: questionSolver,
				timeProvider: timeProvider
			);

			var results = await assignmentQuestionService.GetSubmissionsAsync
			(
				"Class1",
				assignmentId: 1,
				userId: 2,
				submissionDate: SubmissionDate
			);

			Assert.True(results.SequenceEqual(submissionResults));
		}

		/// <summary>
		/// Ensures that DeleteSubmissionAsync deletes a submission for a single question.
		/// </summary>
		[Fact]
		public async Task DeleteSubmissionAsync_SingleQuestion_SubmissionDeleted()
		{
			var database = GetDatabaseWithSubmissions().Build();
			var assignment = database.Context
				.Assignments
				.Include(a => a.Questions)
				.Single(a => a.Name == "Unit 1a");
			var user = database.Context.Users.Single();
			database.Reload();

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context
			);

			await assignmentQuestionService.DeleteSubmissionAsync
			(
				"Class1",
				assignment.Id,
				assignment.Questions[0].Id,
				user.Id,
				DueDate
			);

			var submissions = database.Context
				.UserQuestionSubmissions
				.Where
				(
					uqs => uqs.UserQuestionData.AssignmentQuestion.AssignmentId 
						== assignment.Id
				).ToList();
			
			Assert.DoesNotContain(submissions, s => s.DateSubmitted == DueDate);
			Assert.Contains(submissions, s => s.DateSubmitted != DueDate);
		}

		/// <summary>
		/// Ensures that DeleteSubmissionAsync deletes a submission for an assignment
		/// containing multiple questions with combined submissions.
		/// </summary>
		[Fact]
		public async Task DeleteSubmissionAsync_CombinedSubmissions_SubmissionDeleted()
		{
			var database = GetDatabaseWithSubmissions().Build();
			var assignment = database.Context
				.Assignments
				.Include(a => a.Questions)
				.Single(a => a.Name == "Unit 1b");
			var user = database.Context.Users.Single();
			database.Reload();

			var assignmentQuestionService = GetAssignmentQuestionService
			(
				dbContext: database.Context
			);

			await assignmentQuestionService.DeleteSubmissionAsync
			(
				"Class1",
				assignment.Id,
				null /* assignmentQuestionId */,
				user.Id,
				DueDate
			);

			var submissions = database.Context
				.UserQuestionSubmissions
				.Where
				(
					uqs => uqs.UserQuestionData.AssignmentQuestion.AssignmentId
					    == assignment.Id
				).ToList();
			
			Assert.DoesNotContain(submissions, s => s.DateSubmitted == DueDate);
			Assert.Contains(submissions, s => s.DateSubmitted != DueDate);
		}

		/// <summary>
		/// Returns a mock UserQuestionDataLoaderFactory.
		/// </summary>
		private IUserQuestionDataLoaderFactory GetMockUserQuestionDataLoaderFactory(
			string classroomName,
			int assignmentId,
			int userId,
			int[] assignmentQuestionIds)
		{
			var factory = new Mock<IUserQuestionDataLoaderFactory>();

			var store = new UserQuestionDataStore
			(
				assignmentQuestionIds.ToDictionary
				(
					id => id, 
					id => new UserQuestionData()
					{
						AssignmentQuestion = new AssignmentQuestion()
						{
							Id = id
						}
					}
				)
			);

			if (assignmentQuestionIds.Length == 1)
			{
				factory
					.Setup
					(
						m => m.CreateLoaderForSingleQuestion
						(
							classroomName,
							assignmentId,
							assignmentQuestionIds[0],
							userId
						).LoadUserQuestionDataAsync()
					).ReturnsAsync(store);
			}
			else
			{
				factory
					.Setup
					(
						m => m.CreateLoaderForAllAssignmentQuestions
						(
							classroomName,
							assignmentId,
							userId
						).LoadUserQuestionDataAsync()
					).ReturnsAsync(store);
			}

			return factory.Object;
		}


		/// <summary>
		/// Creates a mock assignment due date retriever.
		/// </summary>
		private IAssignmentDueDateRetriever GetMockAssignmentDueDateRetriever(
			string classroomName,
			int assignmentId,
			int userId,
			DateTime? dueDate)
		{
			var retriever = new Mock<IAssignmentDueDateRetriever>();

			retriever
				.Setup
				(
					m => m.GetUserAssignmentDueDateAsync
					(
						classroomName,
						assignmentId,
						userId
					)
				).ReturnsAsync(dueDate);

			return retriever.Object;
		}

		/// <summary>
		/// Creates a mock question solver.
		/// </summary>
		private IQuestionSolver GetMockQuestionSolver(
			IList<QuestionToSolve> questionToSolveResults = null,
			IList<QuestionSubmission> submissionsToGrade = null,
			IList<SubmissionResult> submissionResults = null,
			DateTime? dateSubmitted = null,
			DateTime? dateDue = null)
		{
			var questionSolver = new Mock<IQuestionSolver>();

			if (questionToSolveResults != null)
			{
				foreach (var questionToSolve in questionToSolveResults)
				{
					questionSolver
						.Setup
						(
							m => m.GetQuestionToSolveAsync
							(
								It.Is<UserQuestionDataStore>
								(
									store => store
										.GetLoadedAssignmentQuestionIds()
										.Contains(questionToSolve.AssignmentQuestionId)
								), 
								questionToSolve.AssignmentQuestionId
							)
						)
						.ReturnsAsync(questionToSolve);
				}
			}

			if (submissionsToGrade != null)
			{
				foreach (var submission in submissionsToGrade)
				{
					questionSolver
						.Setup
						(
							m => m.GradeSubmissionAsync
							(
								It.Is<UserQuestionDataStore>
								(
									store => store
										.GetLoadedAssignmentQuestionIds()
										.Contains(submission.AssignmentQuestionId)
								),
								submission,
								dateSubmitted.Value
							)
						).ReturnsAsync
						(
							new GradeSubmissionResult(scoredQuestionResult: null)
						);
				}
			}

			if (submissionResults != null)
			{
				foreach (var submissionResult in submissionResults)
				{
					var assignmentQuestionId = submissionResult
						.QuestionSubmitted
						.AssignmentQuestionId;

					questionSolver
						.Setup
						(
							m => m.GetSubmissionResultAsync
							(
								It.Is<UserQuestionDataStore>
								(
									store => store
										.GetLoadedAssignmentQuestionIds()
										.Contains(assignmentQuestionId)
								),
								assignmentQuestionId,
								dateSubmitted.Value,
								dateDue
							)
						).ReturnsAsync(submissionResult);
				}
			}

			return questionSolver.Object;
		}

		/// <summary>
		/// Creates a time provider.
		/// </summary>
		private ITimeProvider GetMockTimeProvider(DateTime utcNow)
		{
			var timeProvider = new Mock<ITimeProvider>();

			timeProvider
				.Setup(m => m.UtcNow)
				.Returns(utcNow);

			return timeProvider.Object;
		}

		/// <summary>
		/// Returns a question to solve, for the given assignment question ID.
		/// </summary>
		private QuestionToSolve CreateQuestionToSolve(
			int assignmentQuestionId, 
			int remaining = 0)
		{
			return new QuestionToSolve
			(
				assignmentQuestionId,
				name: null,
				question: null,
				seed: null,
				user: null,
				questionSubmission: null,
				interactive: false,
				pastSubmissions: null,
				userQuestionStatus: new UserQuestionStatus
				(
					numAttempts: 1,
					answeredCorrectly: false,
					numAttemptsRemaining: remaining
				), 
				assignmentProgress: null
			);
		}

		/// <summary>
		/// Creates a new question submission.
		/// </summary>
		private QuestionSubmission CreateQuestionSubmission(
			int assignmentQuestionId)
		{
			var submission = new Mock<QuestionSubmission>();

			submission.Object.AssignmentQuestionId = assignmentQuestionId;

			return submission.Object;
		}

		/// <summary>
		/// Creates a new submission result.
		/// </summary>
		private SubmissionResult CreateSubmissionResult(
			int assignmentQuestionId)
		{
			return new SubmissionResult
			(
				CreateQuestionToSolve(assignmentQuestionId),
				questionResult: null,
				scoreWithoutLateness: 0.0,
				scoreWithLateness: 0.0,
				questionPoints: 0.0,
				submissionDate: SubmissionDate
			);
		}

		/// <summary>
		/// Returns a database builder with pre-added questions.
		/// </summary>
		private TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddStudent("User1", "Last1", "First1", "Class1", "Period1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() {Name = "Question1"})
				.AddQuestion("Class1", "Category1", new MethodQuestion() {Name = "Question2"})
				.AddAssignment
				(
					"Class1",
					"Unit 1",
					"Unit 1a",
					sectionDueDates: new Dictionary<string, DateTime>()
					{
						["Period1"] = DueDate
					},
					questionsByCategory: new Dictionary<string, string[]>()
					{
						["Category1"] = new[]
						{
							"Question1"
						}
					}
				)
				.AddAssignment
				(
					"Class1",
					"Unit 1",
					"Unit 1b",
					sectionDueDates: new Dictionary<string, DateTime>()
					{
						["Period1"] = DueDate
					},
					questionsByCategory: new Dictionary<string, string[]>()
					{
						["Category1"] = new[]
						{
							"Question1",
							"Question2"
						}
					},
					combinedSubmissions: true
				);
		}

		/// <summary>
		/// Returns a database builder with pre-added submissions
		/// </summary>
		private TestDatabaseBuilder GetDatabaseWithSubmissions()
		{
			return GetDatabase()
				.AddQuestionSubmission
				(
					"Class1",
					"Category1",
					"Question1",
					"User1",
					"Unit 1a",
					"Contents",
					score: 0.5,
					dateSubmitted: DueDate
				)
				.AddQuestionSubmission
				(
					"Class1",
					"Category1",
					"Question1",
					"User1",
					"Unit 1a",
					"Contents2",
					score: 1.0,
					dateSubmitted: DueDate + TimeSpan.FromDays(1)
				)
				.AddQuestionSubmission
				(
					"Class1",
					"Category1",
					"Question1",
					"User1",
					"Unit 1b",
					"Contents",
					score: 1.0,
					dateSubmitted: DueDate
				)
				.AddQuestionSubmission
				(
					"Class1",
					"Category1",
					"Question2",
					"User1",
					"Unit 1b",
					"Contents2",
					score: 0.5,
					dateSubmitted: DueDate
				)
				.AddQuestionSubmission
				(
					"Class1",
					"Category1",
					"Question2",
					"User1",
					"Unit 1b",
					"Contents2",
					score: 1.0,
					dateSubmitted: DueDate + TimeSpan.FromDays(1)
				);
		}

		/// <summary>
		/// Creates an AssignmentQuestionService object.
		/// </summary>
		private AssignmentQuestionService GetAssignmentQuestionService(
			DatabaseContext dbContext,
			IUserQuestionDataLoaderFactory userQuestionDataLoaderFactory = null,
			IAssignmentDueDateRetriever assignmentDueDateRetriever = null,
			IQuestionSolver questionSolver = null,
			ITimeProvider timeProvider = null)
		{
			return new AssignmentQuestionService
			(
				dbContext,
				userQuestionDataLoaderFactory,
				assignmentDueDateRetriever,
				questionSolver,
				timeProvider
			);
		}
	}
}
