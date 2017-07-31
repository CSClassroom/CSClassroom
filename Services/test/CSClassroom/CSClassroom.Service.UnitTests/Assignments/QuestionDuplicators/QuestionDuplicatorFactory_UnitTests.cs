using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionDuplicators
{
	/// <summary>
	/// Unit tests for the QuestionDuplicatorFactory class.
	/// </summary>
	public class QuestionDuplicatorFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateQuestionDuplicator returns the correct type
		/// for each type of question.
		/// </summary>
		[Theory]
		[InlineData(typeof(ClassQuestion), typeof(ClassQuestionDuplicator))]
		[InlineData(typeof(GeneratedQuestionTemplate), typeof(GeneratedQuestionDuplicator))]
		[InlineData(typeof(MethodQuestion), typeof(MethodQuestionDuplicator))]
		[InlineData(typeof(MultipleChoiceQuestion), typeof(MultipleChoiceQuestionDuplicator))]
		[InlineData(typeof(ProgramQuestion), typeof(ProgramQuestionDuplicator))]
		[InlineData(typeof(ShortAnswerQuestion), typeof(ShortAnswerQuestionDuplicator))]
		public void CreateQuestionDuplicator_ReturnsCorrectType(
			Type questionType,
			Type expectedDuplicatorType)
		{
			var questionDuplicatorFactory = new QuestionDuplicatorFactory
			(
				dbContext: null
			);

			var result = questionDuplicatorFactory.CreateQuestionDuplicator
			(
				(Question)Activator.CreateInstance(questionType)
			);

			Assert.Equal(expectedDuplicatorType, result.GetType());
		}

		/// <summary>
		/// Ensures that CreateQuestionDuplicator returns the correct type
		/// for each type of question.
		/// </summary>
		[Fact]
		public void CreateQuestionDuplicator_RandomlySelectedQuestion_Throws()
		{
			var questionDuplicatorFactory = new QuestionDuplicatorFactory
			(
				dbContext: null
			);

			Assert.Throws<InvalidOperationException>
			(
				() => questionDuplicatorFactory.CreateQuestionDuplicator
				(
					new RandomlySelectedQuestion()
				)
			);
		}
	}
}
