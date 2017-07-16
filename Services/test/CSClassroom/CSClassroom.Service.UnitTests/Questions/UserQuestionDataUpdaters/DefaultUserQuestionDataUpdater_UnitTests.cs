using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// Unit tests for the DefaultUserQuestionDataUpdater class.
	/// </summary>
	public class DefaultUserQuestionDataUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that UpdateAllAsync invokes the correct updaters.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_UpdatersInvoked()
		{
			var userQuestionData = new UserQuestionData();
			var defaultUpdater = new DefaultUserQuestionDataUpdater();

			defaultUpdater.AddToBatch(userQuestionData);
			await defaultUpdater.UpdateAllAsync();
		}
	}
}
