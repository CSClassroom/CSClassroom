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
	/// Unit tests for the AggregateUserQuestionDataUpdater class.
	/// </summary>
	public class AggregateUserQuestionDataUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that UpdateAllAsync invokes the correct updaters.
		/// </summary>
		[Fact]
		public async Task UpdateAllAsync_UpdatersInvoked()
		{
			var userQuestionDatas = Collections.CreateList
			(
				CreateUserQuestionData(0),
				CreateUserQuestionData(1),
				CreateUserQuestionData(2)
			);

			var impl1 = new MockUserQuestionDataUpdater();
			var impl2 = new MockUserQuestionDataUpdater();

			var mockImplFactory = new Mock<IUserQuestionDataUpdaterImplFactory>();

			SetupMockImplFactory(mockImplFactory, userQuestionDatas[0], impl1);
			SetupMockImplFactory(mockImplFactory, userQuestionDatas[1], impl1);
			SetupMockImplFactory(mockImplFactory, userQuestionDatas[2], impl2);

			var aggregateUpdater = new AggregateUserQuestionDataUpdater
			(
				mockImplFactory.Object
			);

			foreach (var uqd in userQuestionDatas)
			{
				aggregateUpdater.AddToBatch(uqd);
			}

			await aggregateUpdater.UpdateAllAsync();

			Assert.True(impl1.VerifyUpdates(Collections.CreateList(0, 1)));
			Assert.True(impl2.VerifyUpdates(Collections.CreateList(2)));
		}

		/// <summary>
		/// Sets up the mock factory to return a given updater for a given 
		/// UserQuestionData object.
		/// </summary>
		private static void SetupMockImplFactory(
			Mock<IUserQuestionDataUpdaterImplFactory> mockImplFactory, 
			UserQuestionData userQuestionData,
			MockUserQuestionDataUpdater impl)
		{
			mockImplFactory
				.Setup
				(
					m => m.GetUserQuestionDataUpdater
					(
						userQuestionData.AssignmentQuestion.Question
					)
				).Returns(impl);
		}

		/// <summary>
		/// Creates a user question data object with the given assignment question ID.
		/// </summary>
		private UserQuestionData CreateUserQuestionData(int questionId)
		{
			return new UserQuestionData()
			{
				AssignmentQuestion = new AssignmentQuestion()
				{
					QuestionId = questionId,
					Question = new MethodQuestion()
					{
						Id = questionId
					}
				}
			};
		}
	}
}
