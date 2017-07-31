using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters;
using MoreLinq;

namespace CSC.CSClassroom.Service.UnitTests.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// A mock user question data updater factory.
	/// </summary>
	public class MockUserQuestionDataUpdaterFactory : IUserQuestionDataUpdaterFactory
	{
		/// <summary>
		/// The mock user question data updater to return.
		/// </summary>
		private readonly MockUserQuestionDataUpdater _userQuestionDataUpdater
			= new MockUserQuestionDataUpdater();

		/// <summary>
		/// Creates a new UserQuestionDataUpdater.
		/// </summary>
		public IUserQuestionDataUpdater CreateUserQuestionDataUpdater()
		{
			return _userQuestionDataUpdater;
		}
		/// <summary>
		/// Verify that update was called with the given question IDs.
		/// </summary>
		public bool VerifyUpdates(IEnumerable<int> questionIds)
		{
			return _userQuestionDataUpdater.VerifyUpdates(questionIds);
		}
	}
}
