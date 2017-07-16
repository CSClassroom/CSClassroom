using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;
using CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// Unit tests for the RandomlySelectedUserQuestionDataUpdater class.
	/// </summary>
	public class RandomlySelectedUserQuestionDataUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that AddToBatch throws when given a UserQuestionData object
		/// corresponding to a question that is not a randomly selected question.
		/// </summary>
		[Fact]
		public void AddToBatch_NotGeneratedQuestionTemplate_Throws()
		{
			var userQuestionData = CreateUserQuestionData
			(
				randomlySelectedQuestion: null, 
				attemptsRemaining: true
			);

			userQuestionData.AssignmentQuestion.Question = new MethodQuestion();

			var updater = CreateUserQuestionDataUpdater(dbContext: null);

			Assert.Throws<InvalidOperationException>
			(
				() => updater.AddToBatch(userQuestionData)
			);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync does not regenerate a question
		/// for which there are no attempts remaining.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_NoCurrentChoiceAndNoAttemptsRemaining_QuestionNotRegenerated()
		{
			var database = GetDatabase().Build();

			var randomlySelectedQuestion = database.Context
				.RandomlySelectedQuestions
				.Include(q => q.ChoicesCategory)
				.First();
			
			var userQuestionData = CreateUserQuestionData
			(
				randomlySelectedQuestion, 
				attemptsRemaining: false,
				previousQuestionId: null
			);

			var updater = CreateUserQuestionDataUpdater(database.Context);
			updater.AddToBatch(userQuestionData);

			await updater.UpdateAllAsync();

			Assert.Null(userQuestionData.Seed);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync does not regenerate a question when 
		/// the the current question choice is still valid.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task UpdateAllAsync_PreviousChoiceStillValid_QuestionNotRegenerated(
			bool attemptsRemaining)
		{
			var database = GetDatabase().Build();

			var randomlySelectedQuestion = database.Context
				.RandomlySelectedQuestions
				.Include(q => q.ChoicesCategory)
				.First();

			var currentChoice = database.Context
				.MethodQuestions
				.First();
			
			var userQuestionData = CreateUserQuestionData
			(
				randomlySelectedQuestion,
				attemptsRemaining: attemptsRemaining,
				previousQuestionId: currentChoice.Id
			);

			var updater = CreateUserQuestionDataUpdater(database.Context);
			updater.AddToBatch(userQuestionData);

			await updater.UpdateAllAsync();

			Assert.Equal(currentChoice.Id, userQuestionData.Seed);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync regenerates a question when the current
		/// seed is null (indicating the question must be regenerated), if there
		/// are attempts remaining.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_NoCurrentChoiceAndAttemptsRemaining_QuestionRegenerated()
		{
			var database = GetDatabase().Build();

			var randomlySelectedQuestion = database.Context
				.RandomlySelectedQuestions
				.Include(q => q.ChoicesCategory)
				.First();

			var choices = database.Context
				.MethodQuestions
				.Select(q => q.Id)
				.ToList();

			var userQuestionData = CreateUserQuestionData
			(
				randomlySelectedQuestion,
				attemptsRemaining: true,
				previousQuestionId: null
			);
			
			var questionSelector = CreateMockQuestionSelector
			(
				userQuestionData,
				choices,
				choices.First()
			);

			var updater = CreateUserQuestionDataUpdater
			(
				database.Context,
				questionSelector
			);

			updater.AddToBatch(userQuestionData);
			await updater.UpdateAllAsync();

			Assert.Equal(choices.First(), userQuestionData.Seed);
			Assert.Null(userQuestionData.LastQuestionSubmission);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync regenerates a question when the current
		/// seed is null (indicating the question must be regenerated), if there
		/// are attempts remaining.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_CurrentChoiceInvalidAndAttemptsRemaining_QuestionRegenerated()
		{
			var database = GetDatabase().Build();

			var randomlySelectedQuestion = database.Context
				.RandomlySelectedQuestions
				.Include(q => q.ChoicesCategory)
				.First();

			var choices = database.Context
				.MethodQuestions
				.Select(q => q.Id)
				.ToList();

			var userQuestionData = CreateUserQuestionData
			(
				randomlySelectedQuestion,
				attemptsRemaining: true,
				previousQuestionId: 12345
			);

			var questionSelector = CreateMockQuestionSelector
			(
				userQuestionData,
				choices,
				choices.First()
			);

			var updater = CreateUserQuestionDataUpdater
			(
				database.Context,
				questionSelector
			);

			updater.AddToBatch(userQuestionData);
			await updater.UpdateAllAsync();

			Assert.Equal(choices.First(), userQuestionData.Seed);
			Assert.Null(userQuestionData.LastQuestionSubmission);
		}

		/// <summary>
		/// Creates a new UserQuestionData object.
		/// </summary>
		private UserQuestionData CreateUserQuestionData(
			RandomlySelectedQuestion randomlySelectedQuestion,
			bool attemptsRemaining = true,
			int? previousQuestionId = null)
		{
			return new UserQuestionData()
			{
				NumAttempts = 1,
				LastQuestionSubmission = "LastQuestionSubmission",
				Seed = previousQuestionId,
				AssignmentQuestion = new AssignmentQuestion()
				{
					Assignment = new Assignment
					{
						MaxAttempts = attemptsRemaining
							? 2
							: 1
					},
					Question = randomlySelectedQuestion
				}
			};
		}

		/// <summary>
		/// Creates a mock question selector.
		/// </summary>
		private IRandomlySelectedQuestionSelector CreateMockQuestionSelector(
			UserQuestionData userQuestionData,
			IList<int> availableQuestionIds,
			int newSeed)
		{
			var mockQuestionSelector = new Mock<IRandomlySelectedQuestionSelector>();

			mockQuestionSelector
				.Setup
				(
					m => m.GetNextQuestionId
					(
						userQuestionData,
						It.Is<IList<int>>
						(
							ids => ids
								.OrderBy(id => id)
								.SequenceEqual
								(
									availableQuestionIds
										.OrderBy(id => id)
								)
						)
					)
				).Returns(newSeed);

			return mockQuestionSelector.Object;
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new RandomlySelectedQuestion()
				{
					ChoicesCategory = new QuestionCategory()
					{
						Questions = Collections.CreateList
						(
							new MethodQuestion() { Id = 100 },
							new MethodQuestion() { Id = 200 },
							new MethodQuestion() { Id = 300 }
						).Cast<Question>().ToList()
					}
				});
		}

		/// <summary>
		/// Creates a new RandomlySelectedUserQuestionDataUpdater.
		/// </summary>
		private RandomlySelectedUserQuestionDataUpdater CreateUserQuestionDataUpdater(
			DatabaseContext dbContext,
			IRandomlySelectedQuestionSelector questionSelector = null)
		{
			return new RandomlySelectedUserQuestionDataUpdater
			(
				dbContext,
				questionSelector
			);
		}
	}
}
