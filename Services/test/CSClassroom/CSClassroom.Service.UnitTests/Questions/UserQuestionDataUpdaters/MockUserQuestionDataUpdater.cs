using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters;

namespace CSC.CSClassroom.Service.UnitTests.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// A mock user question data updater.
	/// </summary>
	public class MockUserQuestionDataUpdater : IUserQuestionDataUpdater
	{
		/// <summary>
		/// A list of question IDs that were added to the batch.
		/// </summary>
		private readonly IList<int> _questionIds = new List<int>();

		/// <summary>
		/// Whether or not update was called.
		/// </summary>
		private bool _updateCalled;

		/// <summary>
		/// Adds a job to the batch that updates the given UserQuestionData object.
		/// </summary>
		public void AddToBatch(UserQuestionData userQuestionData)
		{
			_questionIds.Add(userQuestionData.AssignmentQuestion.QuestionId);
		}

		/// <summary>
		/// Updates all UserQuestionData objects. 
		/// </summary>
		public Task UpdateAllAsync()
		{
			_updateCalled = true;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Verify that update was called with the given question IDs.
		/// </summary>
		public bool VerifyUpdates(IEnumerable<int> questionIds)
		{
			return _updateCalled
				&& _questionIds
				.OrderBy(id => id)
				.SequenceEqual(questionIds.OrderBy(id => id));
		}
	}
}
