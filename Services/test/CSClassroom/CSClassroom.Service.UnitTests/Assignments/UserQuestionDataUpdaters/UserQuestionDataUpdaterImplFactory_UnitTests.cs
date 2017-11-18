using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Unit tests for the UserQuestionDataUpdaterImplFactory class.
	/// </summary>
	public class UserQuestionDataUpdaterImplFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateQuestionLoader returns the correct type
		/// for each type of question.
		/// </summary>
		[Theory]
		[InlineData(typeof(ClassQuestion), typeof(DefaultUserQuestionDataUpdater))]
		[InlineData(typeof(GeneratedQuestionTemplate), typeof(GeneratedUserQuestionDataUpdater))]
		[InlineData(typeof(MethodQuestion), typeof(DefaultUserQuestionDataUpdater))]
		[InlineData(typeof(MultipleChoiceQuestion), typeof(DefaultUserQuestionDataUpdater))]
		[InlineData(typeof(ProgramQuestion), typeof(DefaultUserQuestionDataUpdater))]
		[InlineData(typeof(RandomlySelectedQuestion), typeof(RandomlySelectedUserQuestionDataUpdater))]
		[InlineData(typeof(ShortAnswerQuestion), typeof(DefaultUserQuestionDataUpdater))]
		public void GetUserQuestionDataUpdater_ReturnsCorrectType(
			Type questionType,
			Type expectedLoaderType)
		{
			var implFactory = new UserQuestionDataUpdaterImplFactory
			(
				dbContext: null,
				questionStatusCalculator: null,
				questionGenerator: null,
				seedGenerator: null,
				questionSelector: null,
				timeProvider: null
			);

			var result = implFactory.GetUserQuestionDataUpdater
			(
				(Question)Activator.CreateInstance(questionType)
			);

			Assert.Equal(expectedLoaderType, result.GetType());
		}
	}
}
