using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Service.Assignments.QuestionResolvers;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionResolvers
{
	/// <summary>
	/// Unit tests for the DefaultQuestionResolver class. 
	/// </summary>
	public class DefaultQuestionResolver_UnitTests : QuestionResolverUnitTestBase
	{
		/// <summary>
		/// Ensures that ResolveUnsolvedQuestionAsync returns the actual question.
		/// </summary>
		[Fact]
		public async Task ResolveUnsolvedQuestionAsync_ReturnsCachedGeneratedQuestion()
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
		/// Ensures that ResolveSolvedQuestionAsync returns the actual question.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ResolveSolvedQuestionAsync_ReturnsCachedGeneratedQuestion(
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
