using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionResolvers;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionResolvers
{
	/// <summary>
	/// Unit tests for the DefaultQuestionResolver class. 
	/// </summary>
	public class DefaultQuestionResolver_UnitTests : QuestionResolverUnitTestBase
	{
		/// <summary>
		/// Ensures that ResolveUnsolvedQuestionAsync returns null if no attempts
		/// are remaining.
		/// </summary>
		[Fact]
		public async Task ResolveUnsolvedQuestionAsync_NoAttemptsRemaining_ReturnsNull()
		{
			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining: false,
				question: new MultipleChoiceQuestion()
			);

			var resolver = new DefaultQuestionResolver(userQuestionData);
			var result = await resolver.ResolveUnsolvedQuestionAsync();

			Assert.Null(result);
		}

		/// <summary>
		/// Ensures that ResolveUnsolvedQuestionAsync returns the actual question,
		/// if there are attempts remaining.
		/// </summary>
		[Fact]
		public async Task ResolveUnsolvedQuestionAsync_AttemptsRemaining_ReturnsCachedGeneratedQuestion()
		{
			var resolvedQuestion = new MultipleChoiceQuestion();
			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining: true,
				question: resolvedQuestion
			);
			
			var resolver = new DefaultQuestionResolver(userQuestionData);
			var result = await resolver.ResolveUnsolvedQuestionAsync();

			Assert.Equal(resolvedQuestion, result);
		}

		/// <summary>
		/// Ensures that ResolveSolvedQuestionAsync returns the actual question,
		/// regardless of whether or not there are attempts remaining.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ResolveSolvedQuestionAsync_AttemptsRemaining_ReturnsCachedGeneratedQuestion(
			bool attemptsRemaining)
		{
			var resolvedQuestion = new MultipleChoiceQuestion();
			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining,
				question: resolvedQuestion
			);

			var resolver = new DefaultQuestionResolver(userQuestionData);
			var result = await resolver.ResolveSolvedQuestionAsync
			(
				CreateUserQuestionSubmission()	
			);

			Assert.Equal(resolvedQuestion, result);
		}
	}
}
