using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
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
			var userQuestionData = CreateUserQuestionData(attemptsRemaining: true);
			userQuestionData.AssignmentQuestion.Question = new MethodQuestion();

			var updater = CreateUserQuestionDataUpdater();

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
			var userQuestionData = CreateUserQuestionData(attemptsRemaining: false);
			var updater = CreateUserQuestionDataUpdater();

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
				attemptsRemaining: attemptsRemaining,
				previouslyUsedSeed: 100,
				dateTemplateModified: DateTime.MinValue,
				dateGenerated: DateTime.MaxValue
			);

			var updater = CreateUserQuestionDataUpdater();
			updater.AddToBatch(userQuestionData);

			await updater.UpdateAllAsync();

			Assert.Null(userQuestionData.CachedQuestionData);
		}

		/// <summary>
		/// Ensures that UpdateAllAsync regenerates a question when the current
		/// seed is null (indicating the question must be regenerated), if there
		/// are attempts remaining.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_CurrentSeedIsNullAndAttemptsRemaining_QuestionRegenerated()
		{
			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining: true,
				templateId: 1,
				numSeeds: 100
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
				attemptsRemaining: true,
				templateId: 1,
				numSeeds: 100,
				previouslyUsedSeed: 150,
				dateTemplateModified: DateTime.MaxValue,
				dateGenerated: DateTime.MinValue
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
				attemptsRemaining: true,
				templateId: 1,
				numSeeds: 100,
				previouslyUsedSeed: 75,
				dateTemplateModified: DateTime.MaxValue,
				dateGenerated: DateTime.MinValue
			);

			var questionGenerator = CreateMockQuestionGenerator();
			var updater = CreateUserQuestionDataUpdater
			(
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
				attemptsRemaining: true,
				templateId: 1,
				numSeeds: 100
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
			int templateId = 1,
			bool attemptsRemaining = true,
			int? previouslyUsedSeed = null,
			int? numSeeds = null,
			DateTime? dateTemplateModified = null,
			DateTime? dateGenerated = null)
		{
			return new UserQuestionData()
			{
				NumAttempts = 1,
				Seed = previouslyUsedSeed,
				CachedQuestionDataTime = dateGenerated,
				LastQuestionSubmission = "LastQuestionSubmission",
				AssignmentQuestion = new AssignmentQuestion()
				{
					Assignment = new Assignment
					{
						MaxAttempts = attemptsRemaining
							? 2
							: 1
					},
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
			IQuestionGenerator questionGenerator = null,
			IGeneratedQuestionSeedGenerator seedGenerator = null)
		{
			var mockTimeProvider = new Mock<ITimeProvider>();
			mockTimeProvider
				.Setup(m => m.UtcNow)
				.Returns(GenerationTime);

			return new GeneratedUserQuestionDataUpdater
			(
				questionGenerator,
				seedGenerator,
				mockTimeProvider.Object
			);
		}
	}
}
