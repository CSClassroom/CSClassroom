﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.Assignments.QuestionResolvers;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionResolvers
{
	/// <summary>
	/// Unit tests for the RandomlySelectedQuestionResolver class. 
	/// </summary>
	public class RandomlySelectedQuestionResolver_UnitTests : QuestionResolverUnitTestBase
	{
		/// <summary>
		/// Ensures that ResolveUnsolvedQuestionAsync loads the question whose
		/// ID is equal to the seed stored in the UserQuestionData object.
		/// </summary>
		[Fact]
		public async Task ResolveUnsolvedQuestionAsync_ReturnsCachedGeneratedQuestion()
		{
			var database = GetDatabase().Build();

			var questionId = database.Context
				.Questions
				.First()
				.Id;

			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining: true,
				seed: questionId
			);
			
			var questionLoaderFactory = new Mock<IQuestionLoaderFactory>();
			questionLoaderFactory
				.Setup
				(
					m => m.CreateQuestionLoader
					(
						It.Is<Question>(q => q.Id == questionId)
					).LoadQuestionAsync()
				).Returns(Task.CompletedTask);

			var resolver = new RandomlySelectedQuestionResolver
			(
				userQuestionData,
				database.Context,
				questionLoaderFactory.Object
			);

			var result = await resolver.ResolveUnsolvedQuestionAsync();

			Assert.Equal(questionId, result.Id);
		}

		/// <summary>
		/// Ensures that ResolveSolvedQuestionAsync loads the question whose
		/// ID is equal to the seed stored in the user's submission.
		/// </summary>
		[Fact]
		public async Task ResolveSolvedQuestionAsync_ReturnsCachedGeneratedQuestion()
		{
			var database = GetDatabase().Build();

			var questionId = database.Context
				.Questions
				.First()
				.Id;

			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining: true,
				seed: 12345
			);

			var questionLoaderFactory = new Mock<IQuestionLoaderFactory>();
			questionLoaderFactory
				.Setup
				(
					m => m.CreateQuestionLoader
					(
						It.Is<Question>(q => q.Id == questionId)
					).LoadQuestionAsync()
				).Returns(Task.CompletedTask);

			var resolver = new RandomlySelectedQuestionResolver
			(
				userQuestionData,
				database.Context,
				questionLoaderFactory.Object
			);

			var result = await resolver.ResolveSolvedQuestionAsync
			(
				CreateUserQuestionSubmission(seed: questionId)	
			);

			Assert.Equal(questionId, result.Id);
		}

		/// <summary>
		/// Returns a database builder with pre-added questions.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion() {Name = "Question1"});
		}
	}
}
