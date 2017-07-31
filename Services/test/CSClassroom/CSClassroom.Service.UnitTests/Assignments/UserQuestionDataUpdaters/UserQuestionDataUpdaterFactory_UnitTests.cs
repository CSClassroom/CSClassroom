using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Unit tests for the UserQuestionDataLoaderFactory class.
	/// </summary>
	public class UserQuestionDataUpdaterFactory_UnitTests
	{
		/// <summary>
		/// Ensures that CreateUserQuestionDataUpdater returns a valid updater.
		/// </summary>
		[Fact]
		public void CreateLoaderForSingleQuestion_ReturnsLoader()
		{
			var factory = new UserQuestionDataUpdaterFactory
			(
				dbContext: null,
				questionGenerator: null,
				seedGenerator: null,
				questionSelector: null,
				timeProvider: null
			);

			var result = factory.CreateUserQuestionDataUpdater();

			Assert.NotNull(result);
		}
	}
}
