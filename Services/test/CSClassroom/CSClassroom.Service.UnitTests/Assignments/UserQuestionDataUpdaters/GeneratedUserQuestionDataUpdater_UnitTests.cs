using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.Assignments.QuestionSolvers;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Unit tests for the GeneratedUserQuestionDataUpdater class.
	/// </summary>
	public class GeneratedUserQuestionDataUpdater_UnitTests
	{
		/// <summary>
		/// The time each question is generated.
		/// </summary>
		private readonly DateTime GenerationTime = new DateTime(2017, 1, 1, 0, 0, 0);

		/// <summary>
		/// Ensures that AddToBatch throws when given a UserQuestionData object
		/// corresponding to a question that is not a generated question template.
		/// </summary>
		[Fact]
		public void AddToBatch_NotGeneratedQuestionTemplate_Throws()
		{
			var userQuestionData = CreateUserQuestionData();
			userQuestionData.AssignmentQuestion.Question = new MethodQuestion();

			var statusCalculator = GetMockQuestionStatusCalculator(userQuestionData);
			var updater = CreateUserQuestionDataUpdater(statusCalculator);

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
		public async Task UpdateAllAsync_CurrentSeedIsNullAndNoAttemptsRemaining_QuestionNotRegenerated()
		{
			var userQuestionData = CreateUserQuestionData();

			var statusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				attemptsRemaining: false
			);
			
			var updater = CreateUserQuestionDataUpdater(statusCalculator);

			updater.AddToBatch(userQuestionData);
			await updater.UpdateAllAsync();

			Assert.Null(userQuestionData.CachedQuestionData);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync does not regenerate a question when 
		/// the template hasn't been modified since it was generated.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task UpdateAllAsync_PreviousSeedStillValid_QuestionNotRegenerated(
			bool attemptsRemaining)
		{
			var userQuestionData = CreateUserQuestionData
			(
				previouslyUsedSeed: 100,
				dateTemplateModified: DateTime.MinValue,
				dateGenerated: DateTime.MaxValue
			);

			var statusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				attemptsRemaining
			);
			
			var updater = CreateUserQuestionDataUpdater(statusCalculator);
			updater.AddToBatch(userQuestionData);

			await updater.UpdateAllAsync();

			Assert.Null(userQuestionData.CachedQuestionData);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync regenerates a question when the current
		/// seed is null (indicating the question must be regenerated), if there
		/// are attempts remaining (or if the user is an admin).
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_CurrentSeedIsNullAndAttemptsRemaining_QuestionRegenerated()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numSeeds: 100
			);

			var statusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				attemptsRemaining: true
			);

			var questionGenerator = CreateMockQuestionGenerator();
			var seedGenerator = CreateMockSeedGenerator
			(
				userQuestionData,
				maxSeed: 100,
				newSeed: 50
			);

			var updater = CreateUserQuestionDataUpdater
			(
				statusCalculator,
				questionGenerator,
				seedGenerator
			);

			updater.AddToBatch(userQuestionData);
			await updater.UpdateAllAsync();

			Assert.Equal("SerializedQuestion1", userQuestionData.CachedQuestionData);
			Assert.Equal(50, userQuestionData.Seed);
			Assert.Equal(GenerationTime, userQuestionData.CachedQuestionDataTime);
			Assert.Null(userQuestionData.LastQuestionSubmission);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync regenerates a question with a new seed when
		/// the template was modified, if the previous generation used a seed that
		/// is now too large. 
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_TemplateModifiedAndCurrentSeedTooBig_QuestionRegeneratedWithNewSeed()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numSeeds: 100,
				previouslyUsedSeed: 150,
				dateTemplateModified: DateTime.MaxValue,
				dateGenerated: DateTime.MinValue
			);

			var statusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				attemptsRemaining: true
			);

			var questionGenerator = CreateMockQuestionGenerator();
			var seedGenerator = CreateMockSeedGenerator
			(
				userQuestionData,
				maxSeed: 100,
				newSeed: 50
			);

			var updater = CreateUserQuestionDataUpdater
			(
				statusCalculator,
				questionGenerator,
				seedGenerator
			);

			updater.AddToBatch(userQuestionData);
			await updater.UpdateAllAsync();

			Assert.Equal("SerializedQuestion1", userQuestionData.CachedQuestionData);
			Assert.Equal(50, userQuestionData.Seed);
			Assert.Equal(GenerationTime, userQuestionData.CachedQuestionDataTime);
			Assert.Null(userQuestionData.LastQuestionSubmission);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync regenerates a question with the same seed when
		/// the template was modified, if the previous generation used a seed that
		/// remains valid.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_TemplateModifiedAndCurrentSeedValid_QuestionRegeneratedWithSameSeed()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numSeeds: 100,
				previouslyUsedSeed: 75,
				dateTemplateModified: DateTime.MaxValue,
				dateGenerated: DateTime.MinValue
			);

			var statusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				attemptsRemaining: true
			);
			
			var questionGenerator = CreateMockQuestionGenerator();
			var updater = CreateUserQuestionDataUpdater
			(
				statusCalculator,
				questionGenerator
			);

			updater.AddToBatch(userQuestionData);
			await updater.UpdateAllAsync();

			Assert.Equal("SerializedQuestion1", userQuestionData.CachedQuestionData);
			Assert.Equal(75, userQuestionData.Seed);
			Assert.Equal(GenerationTime, userQuestionData.CachedQuestionDataTime);
			Assert.NotNull(userQuestionData.LastQuestionSubmission);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync throws upon a failed attempt to regenerate
		/// a question.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_RegenerationFails_Throws()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numSeeds: 100
			);

			var statusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				attemptsRemaining: true
			);
			
			var questionGenerator = CreateMockQuestionGenerator(fails: true);
			var seedGenerator = CreateMockSeedGenerator
			(
				userQuestionData,
				maxSeed: 100,
				newSeed: 50
			);

			var updater = CreateUserQuestionDataUpdater
			(
				statusCalculator,
				questionGenerator,
				seedGenerator
			);

			updater.AddToBatch(userQuestionData);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await updater.UpdateAllAsync()
			);
		}

		/// <summary>
		/// Creates a new UserQuestionData object.
		/// </summary>
		private UserQuestionData CreateUserQuestionData(
			int? previouslyUsedSeed = null,
			int? numSeeds = null,
			DateTime? dateTemplateModified = null,
			DateTime? dateGenerated = null)
		{
			return new UserQuestionData()
			{
				Seed = previouslyUsedSeed,
				CachedQuestionDataTime = dateGenerated,
				LastQuestionSubmission = "LastQuestionSubmission",
				AssignmentQuestion = new AssignmentQuestion()
				{
					Question = new GeneratedQuestionTemplate()
					{
						Id = 1,
						DateModified = dateTemplateModified 
							?? DateTime.MinValue,
						NumSeeds = numSeeds
					}
				}
			};
		}

		/// <summary>
		/// Returns a mock QuestionStatusCalculator.
		/// </summary>
		private IQuestionStatusCalculator GetMockQuestionStatusCalculator(
			UserQuestionData userQuestionData,
			bool attemptsRemaining = true)
		{
			var statusCalculator = new Mock<IQuestionStatusCalculator>();
			statusCalculator
				.Setup(m => m.GetQuestionStatus(userQuestionData))
				.Returns
				(
					new UserQuestionStatus
					(
						numAttempts: 0, 
						answeredCorrectly: false, 
						numAttemptsRemaining: attemptsRemaining ? (int?)null : 0
					)
				);
			
			return statusCalculator.Object;
		}

		/// <summary>
		/// Creates a mock seed generator.
		/// </summary>
		private IGeneratedQuestionSeedGenerator CreateMockSeedGenerator(
			UserQuestionData userQuestionData,
			int maxSeed,
			int newSeed)
		{
			var mockSeedGenerator = new Mock<IGeneratedQuestionSeedGenerator>();

			mockSeedGenerator
				.Setup(m => m.GenerateSeed(userQuestionData, maxSeed))
				.Returns(newSeed);

			return mockSeedGenerator.Object;
		}

		/// <summary>
		/// Returns a new mock question generator.
		/// </summary>
		private IQuestionGenerator CreateMockQuestionGenerator(
			bool fails = false)
		{
			var mockQuestionGenerator = new Mock<IQuestionGenerator>();

			mockQuestionGenerator
				.Setup
				(
					m => m.GenerateQuestionAsync
					(
						It.IsAny<GeneratedQuestionTemplate>(),
						It.IsAny<int>()
					)
				)
				.Returns<GeneratedQuestionTemplate, int>
				(
					(generatedQuestionTemplate, seed) => Task.FromResult
					(
						fails
							? new QuestionGenerationResult("Failed to regenerate") 
							: new QuestionGenerationResult
								(
									$"SerializedQuestion{generatedQuestionTemplate.Id}",
									fullGeneratorFileContents: null,
									fullGeneratorFileLineOffset: 0,
									seed: seed
								)
					)
				);

			return mockQuestionGenerator.Object;
		}

		/// <summary>
		/// Creates a new GeneratedUserQuestionDataUpdater.
		/// </summary>
		private GeneratedUserQuestionDataUpdater CreateUserQuestionDataUpdater(
			IQuestionStatusCalculator questionStatusCalculator = null,
			IQuestionGenerator questionGenerator = null,
			IGeneratedQuestionSeedGenerator seedGenerator = null)
		{
			var mockTimeProvider = new Mock<ITimeProvider>();
			mockTimeProvider
				.Setup(m => m.UtcNow)
				.Returns(GenerationTime);

			return new GeneratedUserQuestionDataUpdater
			(
				questionStatusCalculator,
				questionGenerator,
				seedGenerator,
				mockTimeProvider.Object
			);
		}
	}
}
