using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Service.Questions.UserQuestionDataLoaders;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.UserQuestionDataLoaders
{
	/// <summary>
	/// Unit tests for the UserQuestionDataLoaderFactory class.
	/// </summary>
	public class UserQuestionDataLoaderFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateLoaderForSingleQuestion returns a valid loader.
		/// </summary>
		[Fact]
		public void CreateLoaderForSingleQuestion_ReturnsLoader()
		{
			var factory = new UserQuestionDataLoaderFactory
			(
				dbContext: null,
				questionLoaderFactory: null,
				userQuestionDataUpdaterFactory: null
			);

			var result = factory.CreateLoaderForSingleQuestion
			(
				"Class1",
				assignmentId: 1,
				assignmentQuestionId: 1,
				userId: 1
			);

			Assert.NotNull(result);
		}

		/// <summary>
		/// Ensures that CreateLoaderForAllAssignmentQuestions returns a valid loader.
		/// </summary>
		[Fact]
		public void CreateLoaderForAllAssignmentQuestions_ReturnsLoader()
		{
			var factory = new UserQuestionDataLoaderFactory
			(
				dbContext: null,
				questionLoaderFactory: null,
				userQuestionDataUpdaterFactory: null
			);

			var result = factory.CreateLoaderForAllAssignmentQuestions
			(
				"Class1",
				assignmentId: 1,
				userId: 1
			);

			Assert.NotNull(result);
		}
	}
}
