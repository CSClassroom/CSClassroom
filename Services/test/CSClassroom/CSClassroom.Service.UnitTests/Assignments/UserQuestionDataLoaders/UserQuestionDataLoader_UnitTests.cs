using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters;
using CSC.CSClassroom.Service.UnitTests.Assignments.UserQuestionDataUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.UserQuestionDataLoaders
{
	/// <summary>
	/// Unit tests for the UserQuestionDataLoader class.
	/// </summary>
	public class UserQuestionDataLoader_UnitTests
	{
		/// <summary>
		/// Verifies that all questions are loaded.
		/// </summary>
		[Fact]
		public async Task LoadUserQuestionDataAsync_QuestionsLoaded()
		{
			var database = GetDatabase().Build();

			var questionLoaderFactory = GetMockQuestionLoaderFactory();
			var userQuestionDataLoader = CreateUserQuestionDataLoader
			(
				database.Context,
				null /*questionId*/,
				questionLoaderFactory.Object
			);

			var results = await userQuestionDataLoader.LoadUserQuestionDataAsync();

			foreach (var question in database.Context.Questions)
			{
				questionLoaderFactory.Verify(GetLoadQuestionExpression(question));
			}

			Assert.True
			(
				results.GetLoadedAssignmentQuestionIds()
					.OrderBy(id => id)
					.SequenceEqual
					(
						database.Context
							.Questions
							.Select(aq => aq.Id)
							.OrderBy(id => id)
					)
			);
		}

		/// <summary>
		/// Verifies that each loaded question was updated through a 
		/// UserQuestionDataUpdater.
		/// </summary>
		[Fact]
		public async Task LoadUserQuestionDataAsync_UserQuestionDataUpdated()
		{
			var database = GetDatabase().Build();
			
			var mockUserQuestionDataUpdaterFactory 
				= new MockUserQuestionDataUpdaterFactory();

			var userQuestionDataLoader = CreateUserQuestionDataLoader
			(
				database.Context,
				null /*questionId*/,
				userQuestionDataUpdaterFactory: mockUserQuestionDataUpdaterFactory
			);

			await userQuestionDataLoader.LoadUserQuestionDataAsync();

			Assert.True
			(
				mockUserQuestionDataUpdaterFactory.VerifyUpdates
				(
					database.Context
						.AssignmentQuestions
						.Select(q => q.Id)
				)
			);
		}

		/// <summary>
		/// Verifies that submissions are not loaded for a single question that
		/// only supports interactive submissions.
		/// </summary>
		[Fact]
		public async Task LoadUserQuestionDataAsync_InteractiveOnlyQuestion_SubmissionsNotLoaded()
		{
			var database = GetDatabase(includeSubmissions: true)
				.Build();

			var interactiveOnlyAssignmentQuestionId = database.Context
				.AssignmentQuestions
				.First(aq => aq.Question.UnsupportedSolver(QuestionSolverType.NonInteractive))
				.Id;

			database.Reload();

			var userQuestionDataLoader = CreateUserQuestionDataLoader
			(
				database.Context,
				interactiveOnlyAssignmentQuestionId
			);

			var results = await userQuestionDataLoader.LoadUserQuestionDataAsync();

			Assert.Equal(1, results.GetLoadedAssignmentQuestionIds().Count);

			var uqd = results.GetUserQuestionData(interactiveOnlyAssignmentQuestionId);
			Assert.Null(uqd.Submissions);
		}

		/// <summary>
		/// Verifies that submissions are loaded for a single question that
		/// supports non-interactive submissions.
		/// </summary>
		[Fact]
		public async Task LoadUserQuestionDataAsync_NotInteractiveOnlyQuestion_SubmissionsLoaded()
		{
			var database = GetDatabase(includeSubmissions: true)
				.Build();

			var nonInteractiveOnlyAssignmentQuestionId = database.Context
				.AssignmentQuestions
				.First(aq => aq.Question.SupportedSolver(QuestionSolverType.NonInteractive))
				.Id;

			database.Reload();

			var userQuestionDataLoader = CreateUserQuestionDataLoader
			(
				database.Context,
				nonInteractiveOnlyAssignmentQuestionId
			);

			var results = await userQuestionDataLoader.LoadUserQuestionDataAsync();

			Assert.Equal(1, results.GetLoadedAssignmentQuestionIds().Count);

			var uqd = results.GetUserQuestionData(nonInteractiveOnlyAssignmentQuestionId);
			Assert.Equal(1, uqd.Submissions.Count);
		}

		/// <summary>
		/// Verifies that user question data objects are returned.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task LoadUserQuestionDataAsync_UserQuestionDataReturned(
			bool previousSubmissions)
		{
			var database = GetDatabase(previousSubmissions)
				.Build();

			var questionLoaderFactory = GetMockQuestionLoaderFactory();
			var userQuestionDataLoader = CreateUserQuestionDataLoader
			(
				database.Context,
				null /*questionId*/,
				questionLoaderFactory.Object
			);

			var results = await userQuestionDataLoader.LoadUserQuestionDataAsync();
			database.Context.SaveChanges();

			Assert.Equal
			(
				database.Context.AssignmentQuestions.Count(), 
				results.GetLoadedAssignmentQuestionIds().Count
			);

			foreach (var assignmentQuestionId in results.GetLoadedAssignmentQuestionIds())
			{
				var uqd = database.Context
					.UserQuestionData
					.Single(u => u.AssignmentQuestionId == assignmentQuestionId);

				Assert.NotNull(uqd);
				Assert.Equal(uqd, results.GetUserQuestionData(assignmentQuestionId));
				Assert.Equal(database.Context.Users.First().Id, uqd.UserId);
			}
		}

		/// <summary>
		/// Creates the user question data loader to test.
		/// </summary>
		private UserQuestionDataLoader CreateUserQuestionDataLoader(
			DatabaseContext dbContext,
			int? questionId = null,
			IQuestionLoaderFactory questionLoaderFactory = null,
			IUserQuestionDataUpdaterFactory userQuestionDataUpdaterFactory = null)
		{
			return new UserQuestionDataLoader
			(
				dbContext,
				questionLoaderFactory ?? GetMockQuestionLoaderFactory().Object,
				userQuestionDataUpdaterFactory ?? new MockUserQuestionDataUpdaterFactory(),
				"Class1",
				dbContext.Assignments.First().Id,
				dbContext.Users.First().Id,
				GetAssignmentQuestionFilter(questionId),
				GetUserQuestionDataFilter(questionId)
			);
		}

		/// <summary>
		/// Returns a mock question loader factory.
		/// </summary>
		private Mock<IQuestionLoaderFactory> GetMockQuestionLoaderFactory()
		{
			var loaderFactory = new Mock<IQuestionLoaderFactory>();

			loaderFactory
				.Setup(GetLoadQuestionExpression())
				.Returns(Task.CompletedTask);

			return loaderFactory;
		}

		/// <summary>
		/// Returns the expression to load a question.
		/// </summary>
		private Expression<Func<IQuestionLoaderFactory, Task>> GetLoadQuestionExpression(
			Question question = null)
		{
			if (question != null)
			{
				return loaderFactory => loaderFactory
					.CreateQuestionLoader(question)
					.LoadQuestionAsync();
			}
			else
			{
				return loaderFactory => loaderFactory
					.CreateQuestionLoader(It.IsNotNull<Question>())
					.LoadQuestionAsync();
			}
		}

		/// <summary>
		/// Returns an assignment question filter.
		/// </summary>
		Expression<Func<AssignmentQuestion, bool>> GetAssignmentQuestionFilter(
			int? assignmentQuestionId)
		{
			if (assignmentQuestionId.HasValue)
			{
				return assignmentQuestion => assignmentQuestion.Id == assignmentQuestionId;
			}
			else
			{
				return assignmentQuestion => true;
			}
		}

		/// <summary>
		/// Returns a UserQuestionData filter.
		/// </summary>
		Expression<Func<UserQuestionData, bool>> GetUserQuestionDataFilter(
			int? assignmentQuestionId)
		{
			if (assignmentQuestionId.HasValue)
			{
				return userQuestionData => userQuestionData.AssignmentQuestionId == assignmentQuestionId;
			}
			else
			{
				return userQuestionData => true;
			}
		}

		/// <summary>
		/// Returns a database builder with pre-added assignments.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase(bool includeSubmissions = false)
		{
			var builder = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddStudent("User1", "Last1", "First1", "Class1", "Period1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() {Name = "Question1"})
				.AddQuestion("Class1", "Category1", new RandomlySelectedQuestion() {Name = "Question2"})
				.AddAssignment
				(
					"Class1",
					"Unit 1",
					"Unit 1a",
					sectionDueDates: new Dictionary<string, DateTime>() { },
					questionsByCategory: new Dictionary<string, string[]>()
					{
						["Category1"] = new[]
						{
							"Question1",
							"Question2"
						}
					}
				);

			if (includeSubmissions)
			{
				builder
					.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "Unit 1a", "PS", 0.0, DateTime.MinValue)
					.AddQuestionSubmission("Class1", "Category1", "Question2", "User1", "Unit 1a", "PS", 0.0, DateTime.MinValue);
			}

			return builder;
		}
	}
}
