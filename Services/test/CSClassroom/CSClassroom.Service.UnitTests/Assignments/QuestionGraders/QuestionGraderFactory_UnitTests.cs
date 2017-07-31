using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionGraders;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionGraders
{
	/// <summary>
	/// Unit tests for the QuestionGraderFactory class.
	/// </summary>
	public class QuestionGraderFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateQuestionGrader returns the correct type
		/// for each type of question.
		/// </summary>
		[Theory]
		[InlineData(typeof(ClassQuestion), typeof(ClassQuestionGrader))]
		[InlineData(typeof(MethodQuestion), typeof(MethodQuestionGrader))]
		[InlineData(typeof(MultipleChoiceQuestion), typeof(MultipleChoiceQuestionGrader))]
		[InlineData(typeof(ProgramQuestion), typeof(ProgramQuestionGrader))]
		[InlineData(typeof(ShortAnswerQuestion), typeof(ShortAnswerQuestionGrader))]
		public void CreateQuestionGrader_ReturnsCorrectType(
			Type questionType,
			Type expectedGraderType)
		{
			var questionGraderFactory = new QuestionGraderFactory
			(
				codeRunnerService: null
			);

			var result = questionGraderFactory.CreateQuestionGrader
			(
				(Question)Activator.CreateInstance(questionType)
			);

			Assert.Equal(expectedGraderType, result.GetType());
		}
	}
}
