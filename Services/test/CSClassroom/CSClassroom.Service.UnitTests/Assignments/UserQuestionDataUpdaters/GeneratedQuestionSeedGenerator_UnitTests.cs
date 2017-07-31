using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// Unit tests for the GeneratedQuestionSeedGenerator class.
	/// </summary>
	public class GeneratedQuestionSeedGenerator_UnitTests
	{
		/// <summary>
		/// Ensures that GenerateSeed generates a random number in the range
		/// [0, maxSeed) when there are no existing submissions.
		/// </summary>
		[Theory]
		[InlineData(0 /*randomNumber*/, 0 /*expectedSeed*/)]
		[InlineData(50 /*randomNumber*/, 50 /*expectedSeed*/)]
		[InlineData(99 /*randomNumber*/, 99 /*expectedSeed*/)]
		[InlineData(100 /*randomNumber*/, 0 /*expectedSeed*/)]
		public void GenerateSeed_NoSubmissions_GeneratesValidRandomSeed(
			int randomNumber, int expectedSeed)
		{
			var userQuestionData = CreateUserQuestionData();
			var seedGenerator = CreateSeedGenerator(randomNumber);

			var result = seedGenerator.GenerateSeed
			(
				userQuestionData,
				numSeeds: 100
			);

			Assert.Equal(expectedSeed, result);
		}

		/// <summary>
		/// Ensures that GenerateSeed generates a random number in the range
		/// [0, maxSeed) that was not previously used in a submission.
		/// </summary>
		[Theory]
		[InlineData(new[] { 1 } /*randomNumbers*/, 1 /*expectedSeed*/)]
		[InlineData(new[] { 0, 1 } /*randomNumber*/, 1 /*expectedSeed*/)]
		[InlineData(new[] { 0, 2, 4, 3 } /*randomNumber*/, 3 /*expectedSeed*/)]
		[InlineData(new[] { 0, 2, 4, 5, 7, 9, 11 } /*randomNumber*/, 1 /*expectedSeed*/)]
		public void GenerateSeed_SomePreviousSubmissions_GeneratesValidUnusedRandomSeed(
			int[] randomNumbers, int expectedSeed)
		{
			var previousSeeds = new int[] { 0, 2, 4 };
			var userQuestionData = CreateUserQuestionData(previousSeeds);
			var seedGenerator = CreateSeedGenerator(randomNumbers);

			var result = seedGenerator.GenerateSeed
			(
				userQuestionData,
				numSeeds: 5
			);

			Assert.Equal(expectedSeed, result);
		}

		/// <summary>
		/// Ensures that GenerateSeed returns the least frequently used seed when
		/// all seeds were used in previous submissions.
		/// </summary>
		[Fact]
		public void GenerateSeed_AllSeedsInPreviousSubmissions_GeneratesLeastFrequentlyUsedSeed()
		{
			var previousSeeds = new int[] { 0, 0, 1, 1, 2, 3, 3, 4, 4 };
			var userQuestionData = CreateUserQuestionData(previousSeeds);
			var seedGenerator = CreateSeedGenerator();

			var result = seedGenerator.GenerateSeed
			(
				userQuestionData,
				numSeeds: 5
			);

			Assert.Equal(2, result);
		}

		/// <summary>
		/// Creates a new UserQuestionData object, with submissions
		/// for each of the given seeds.
		/// </summary>
		private UserQuestionData CreateUserQuestionData(
			int[] previouslyUsedSeeds = null)
		{
			return new UserQuestionData()
			{
				Submissions = previouslyUsedSeeds?.Select
				(
					seed => new UserQuestionSubmission()
					{
						Seed = seed
					}
				)?.ToList()
			};
		}

		/// <summary>
		/// Creates a seed generator.
		/// </summary>
		private GeneratedQuestionSeedGenerator CreateSeedGenerator(
			params int[] randomNumbers)
		{
			if (randomNumbers.Any())
			{
				var mockRng = new Mock<IRandomNumberProvider>();
				var sequence = mockRng.SetupSequence(m => m.NextInt());
				foreach (var randomNumber in randomNumbers)
				{
					sequence = sequence.Returns(randomNumber);
				}

				return new GeneratedQuestionSeedGenerator(mockRng.Object);
			}
			else
			{
				return new GeneratedQuestionSeedGenerator(randomNumberProvider: null);
			}
		}
	}
}
