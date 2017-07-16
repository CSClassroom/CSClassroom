using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionLoaders;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionLoaders
{
	/// <summary>
	/// Unit tests for the QuestionLoaderFactory class.
	/// </summary>
	public class QuestionLoaderFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateQuestionLoader returns the correct type
		/// for each type of question.
		/// </summary>
		[Theory]
		[InlineData(typeof(ClassQuestion), typeof(ClassQuestionLoader))]
		[InlineData(typeof(GeneratedQuestionTemplate), typeof(GeneratedQuestionLoader))]
		[InlineData(typeof(MethodQuestion), typeof(MethodQuestionLoader))]
		[InlineData(typeof(MultipleChoiceQuestion), typeof(MultipleChoiceQuestionLoader))]
		[InlineData(typeof(ProgramQuestion), typeof(ProgramQuestionLoader))]
		[InlineData(typeof(RandomlySelectedQuestion), typeof(RandomlySelectedQuestionLoader))]
		[InlineData(typeof(ShortAnswerQuestion), typeof(ShortAnswerQuestionLoader))]
		public void CreateQuestionLoader_ReturnsCorrectType(
			Type questionType,
			Type expectedLoaderType)
		{
			var questionLoaderFactory = new QuestionLoaderFactory
			(
				dbContext: null
			);

			var result = questionLoaderFactory.CreateQuestionLoader
			(
				(Question)Activator.CreateInstance(questionType)
			);

			Assert.Equal(expectedLoaderType, result.GetType());
		}
	}
}
