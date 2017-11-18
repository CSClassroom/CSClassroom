using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Service.Assignments.QuestionResolvers;
using CSC.CSClassroom.Service.Assignments.QuestionSolvers;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionResolvers
{
	/// <summary>
	/// Unit tests for the QuestionResolverFactory class.
	/// </summary>
	public class QuestionResolverFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateQuestionResolver returns the correct type
		/// for each type of question when new attempts are allowed.
		/// </summary>
		[Theory]
		[InlineData(typeof(ClassQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(GeneratedQuestionTemplate), typeof(GeneratedQuestionTemplateResolver))]
		[InlineData(typeof(MethodQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(MultipleChoiceQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(ProgramQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(RandomlySelectedQuestion), typeof(RandomlySelectedQuestionResolver))]
		[InlineData(typeof(ShortAnswerQuestion), typeof(DefaultQuestionResolver))]
		public void CreateQuestionResolver_NewAttemptAllowed_ReturnsCorrectType(
			Type questionType,
			Type expectedResolverType)
		{
			var userQuestionData = new UserQuestionData()
			{
				AssignmentQuestion = new AssignmentQuestion()
				{
					Question = (Question) Activator.CreateInstance(questionType)
				}
			};
			
			var questionStatusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				allowNewAttempt: true
			);
			
			var questionResolverFactory = new QuestionResolverFactory
			(
				dbContext: null,
				jsonSerializer: null,
				questionLoaderFactory: null,
				questionStatusCalculator: questionStatusCalculator
			);

			var result = questionResolverFactory.CreateQuestionResolver
			(
				userQuestionData
			);

			Assert.Equal(expectedResolverType, result.GetType());
		}

		/// <summary>
		/// Ensures that CreateQuestionResolver returns a NoMoreAttemptsQuestionResolver
		/// when no more attempts are allowed.
		/// </summary>
		public void CreateQuestionResolver_NoNewAttemptAllowed_ReturnsCorrectType()
		{
			var userQuestionData = new UserQuestionData()
			{
				AssignmentQuestion = new AssignmentQuestion()
				{
					Question = new MultipleChoiceQuestion()
				}
			};
			
			var questionStatusCalculator = GetMockQuestionStatusCalculator
			(
				userQuestionData, 
				allowNewAttempt: false
			);
			
			var questionResolverFactory = new QuestionResolverFactory
			(
				dbContext: null,
				jsonSerializer: null,
				questionLoaderFactory: null,
				questionStatusCalculator: questionStatusCalculator
			);

			var result = questionResolverFactory.CreateQuestionResolver
			(
				userQuestionData
			);

			Assert.Equal(typeof(NoMoreAttemptsQuestionResolver), result.GetType());
		}

		/// <summary>
		/// Returns a mock QuestionStatusCalculator.
		/// </summary>
		private IQuestionStatusCalculator GetMockQuestionStatusCalculator(
			UserQuestionData userQuestionData,
			bool allowNewAttempt)
		{
			var questionStatusCalculator = new Mock<IQuestionStatusCalculator>();
			questionStatusCalculator
				.Setup(m => m.GetQuestionStatus(userQuestionData))
				.Returns
				(
					new UserQuestionStatus
					(
						numAttempts: 1, 
						answeredCorrectly: false,
						numAttemptsRemaining: allowNewAttempt ? 1 : 0
					)
				);

			return questionStatusCalculator.Object;
		}
	}
}
