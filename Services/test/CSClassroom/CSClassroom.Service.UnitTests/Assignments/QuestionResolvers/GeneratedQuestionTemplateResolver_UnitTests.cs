using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionResolvers;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionResolvers
{
	/// <summary>
	/// Unit tests for the GeneratedQuestionTemplateResolver class. 
	/// </summary>
	public class GeneratedQuestionTemplateResolver_UnitTests : QuestionResolverUnitTestBase
	{
		/// <summary>
		/// Ensures that ResolveUnsolvedQuestionAsync deserializes and returns
		/// the cached generated question.
		/// </summary>
		[Fact]
		public async Task ResolveUnsolvedQuestionAsync_ReturnsCachedGeneratedQuestion()
		{
			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining: true,
				templateQuestionId: 100,
				cachedQuestionData: "CachedQuestionData"
			);

			var resolvedQuestion = new MultipleChoiceQuestion();
			var jsonSerializer = new Mock<IJsonSerializer>();
			jsonSerializer
				.Setup(m => m.Deserialize<Question>("CachedQuestionData"))
				.Returns(resolvedQuestion);

			var resolver = new GeneratedQuestionTemplateResolver
			(
				userQuestionData,
				jsonSerializer.Object
			);

			var result = await resolver.ResolveUnsolvedQuestionAsync();

			Assert.Equal(resolvedQuestion, result);
			Assert.Equal(100, result.Id);
		}

		/// <summary>
		/// Ensures that ResolveSolvedQuestionAsync deserializes and returns
		/// the cached generated question on the user's submission.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ResolveSolvedQuestionAsync_ReturnsCachedGeneratedQuestion(
			bool attemptsRemaining)
		{
			var userQuestionData = CreateUserQuestionData
			(
				attemptsRemaining,
				templateQuestionId: 100,
				cachedQuestionData: "CachedQuestionData"
			);

			var resolvedQuestion = new MultipleChoiceQuestion();
			var jsonSerializer = new Mock<IJsonSerializer>();
			jsonSerializer
				.Setup(m => m.Deserialize<Question>("SubmissionCachedQuestionData"))
				.Returns(resolvedQuestion);

			var resolver = new GeneratedQuestionTemplateResolver
			(
				userQuestionData,
				jsonSerializer.Object
			);

			var result = await resolver.ResolveSolvedQuestionAsync
			(
				CreateUserQuestionSubmission("SubmissionCachedQuestionData")	
			);

			Assert.Equal(resolvedQuestion, result);
			Assert.Equal(100, result.Id);
		}
	}
}
