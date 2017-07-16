using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// Unit tests for the RandomlySelectedQuestionSelector class.
	/// </summary>
	public class RandomlySelectedQuestionSelector_UnitTests
	{
		/// <summary>
		/// Ensures that GetNextQuestionId a random question in the given
		/// list of available questions.
		/// </summary>
		[Theory]
		[InlineData(0 /*randomNumber*/, 1 /*expectedQuestionId*/)]
		[InlineData(1 /*randomNumber*/, 4 /*expectedQuestionId*/)]
		[InlineData(2 /*randomNumber*/, 9 /*expectedQuestionId*/)]
		[InlineData(3 /*randomNumber*/, 1 /*expectedQuestionId*/)]
		public void GetNextQuestionId_NoSubmissions_SelectsValidRandomQuestion(
			int randomNumber, int expectedQuestionId)
		{
			var userQuestionData = CreateUserQuestionData();
			var questionSelector = CreateQuestionSelector(randomNumber);
			var availableQuestionIds = Collections.CreateList(1, 4, 9);

			var result = questionSelector.GetNextQuestionId
			(
				userQuestionData,
				availableQuestionIds
			);

			Assert.Equal(expectedQuestionId, result);
		}

		/// <summary>
		/// Ensures that GetNextQuestionId a random question from the set of questions
		/// that have not already been used in a previous submission.
		/// </summary>
		[Theory]
		[InlineData(0 /*randomNumber*/, 1 /*expectedQuestionId*/)]
		[InlineData(1 /*randomNumber*/, 9 /*expectedQuestionId*/)]
		[InlineData(2 /*randomNumber*/, 1 /*expectedQuestionId*/)]
		public void GetNextQuestionId_SomePreviousSubmissions_SelectsValidUnusedRandomQuestion(
			int randomNumber, int expectedQuestionId)
		{
			var previouslyUsedQuestions = new int[] { 4 };
			var userQuestionData = CreateUserQuestionData(previouslyUsedQuestions);
			var questionSelector = CreateQuestionSelector(randomNumber);
			var availableQuestionIds = Collections.CreateList(1, 4, 9);

			var result = questionSelector.GetNextQuestionId
			(
				userQuestionData,
				availableQuestionIds
			);

			Assert.Equal(expectedQuestionId, result);
		}

		/// <summary>
		/// Ensures that GetNextQuestionId returns the least frequently used question
		/// when all available questions were used in a previous submission.
		/// </summary>
		[Fact]
		public void GetNextQuestionId_AllQuestionsInPreviousSubmissions_SelectsLeastFrequentlyUsedQuestion()
		{
			var previouslyUsedQuestions = new int[] { 1, 1, 4, 9, 9 };
			var userQuestionData = CreateUserQuestionData(previouslyUsedQuestions);
			var questionSelector = CreateQuestionSelector();
			var availableQuestionIds = Collections.CreateList(1, 4, 9);

			var result = questionSelector.GetNextQuestionId
			(
				userQuestionData,
				availableQuestionIds
			);

			Assert.Equal(4, result);
		}

		/// <summary>
		/// Creates a new UserQuestionData object, with submissions
		/// for each of the given question IDs.
		/// </summary>
		private UserQuestionData CreateUserQuestionData(
			int[] previouslyUsedQuestions = null)
		{
			return new UserQuestionData()
			{
				Submissions = previouslyUsedQuestions?.Select
				(
					questionId => new UserQuestionSubmission()
					{
						Seed = questionId
					}
				)?.ToList()
			};
		}

		/// <summary>
		/// Creates a question selector.
		/// </summary>
		private RandomlySelectedQuestionSelector CreateQuestionSelector(
			int? randomNumber = null)
		{
			if (randomNumber.HasValue)
			{
				var mockRng = new Mock<IRandomNumberProvider>();

				mockRng
					.Setup(m => m.NextInt())
					.Returns(randomNumber.Value);

				return new RandomlySelectedQuestionSelector(mockRng.Object);
			}
			else
			{
				return new RandomlySelectedQuestionSelector(randomNumberProvider: null);
			}
		}
	}
}
