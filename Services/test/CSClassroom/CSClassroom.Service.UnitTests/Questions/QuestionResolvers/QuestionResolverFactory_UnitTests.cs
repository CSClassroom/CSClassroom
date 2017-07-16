using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionResolvers;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionResolvers
{
	/// <summary>
	/// Unit tests for the QuestionResolverFactory class.
	/// </summary>
	public class QuestionResolverFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateQuestionResolver returns the correct type
		/// for each type of question.
		/// </summary>
		[Theory]
		[InlineData(typeof(ClassQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(GeneratedQuestionTemplate), typeof(GeneratedQuestionTemplateResolver))]
		[InlineData(typeof(MethodQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(MultipleChoiceQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(ProgramQuestion), typeof(DefaultQuestionResolver))]
		[InlineData(typeof(RandomlySelectedQuestion), typeof(RandomlySelectedQuestionResolver))]
		[InlineData(typeof(ShortAnswerQuestion), typeof(DefaultQuestionResolver))]
		public void CreateQuestionResolver_ReturnsCorrectType(
			Type questionType,
			Type expectedResolverType)
		{
			var questionResolverFactory = new QuestionResolverFactory
			(
				dbContext: null,
				jsonSerializer: null,
				questionLoaderFactory: null
			);

			var result = questionResolverFactory.CreateQuestionResolver
			(
				new UserQuestionData()
				{
					AssignmentQuestion = new AssignmentQuestion()
					{
						Question = (Question)Activator.CreateInstance(questionType)
					}
				}
			);

			Assert.Equal(expectedResolverType, result.GetType());
		}
	}
}
