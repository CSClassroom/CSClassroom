using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionUpdaters;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionUpdaters
{
	/// <summary>
	/// Unit tests for the QuestionUpdaterFactory class.
	/// </summary>
	public class QuestionUpdaterFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateQuestionUpdater returns the correct type
		/// for each type of question.
		/// </summary>
		[Theory]
		[InlineData(typeof(ClassQuestion), typeof(ClassQuestionUpdater))]
		[InlineData(typeof(GeneratedQuestionTemplate), typeof(GeneratedQuestionUpdater))]
		[InlineData(typeof(MethodQuestion), typeof(MethodQuestionUpdater))]
		[InlineData(typeof(MultipleChoiceQuestion), typeof(MultipleChoiceQuestionUpdater))]
		[InlineData(typeof(ProgramQuestion), typeof(ProgramQuestionUpdater))]
		[InlineData(typeof(RandomlySelectedQuestion), typeof(RandomlySelectedQuestionUpdater))]
		[InlineData(typeof(ShortAnswerQuestion), typeof(ShortAnswerQuestionUpdater))]
		public void CreateQuestionUpdater_ReturnsCorrectType(
			Type questionType,
			Type expectedUpdaterType)
		{
			var questionUpdaterFactory = new QuestionUpdaterFactory
			(
				dbContext: null,
				questionGenerator: null,
				timeProvider: null
			);

			var result = questionUpdaterFactory.CreateQuestionUpdater
			(
				(Question)Activator.CreateInstance(questionType),
				errors: null
			);

			Assert.Equal(expectedUpdaterType, result.GetType());
		}
	}
}
