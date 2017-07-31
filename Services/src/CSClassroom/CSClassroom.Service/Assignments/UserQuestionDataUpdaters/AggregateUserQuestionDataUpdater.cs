using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// A UserQuestionDataUpdater that works for any question type.
	/// </summary>
	public class AggregateUserQuestionDataUpdater : IUserQuestionDataUpdater
	{
		/// <summary>
		/// Creates user question data updaters.
		/// </summary>
		private readonly IUserQuestionDataUpdaterImplFactory _userQuestionDataUpdaterImplFactory;

		/// <summary>
		/// A list of user question data updaters for which jobs were added.
		/// </summary>
		private readonly ICollection<IUserQuestionDataUpdater> _userQuestionDataUpdaters;

		public AggregateUserQuestionDataUpdater(
			IUserQuestionDataUpdaterImplFactory userQuestionDataUpdaterImplFactory)
		{
			_userQuestionDataUpdaterImplFactory = userQuestionDataUpdaterImplFactory;
			_userQuestionDataUpdaters = new HashSet<IUserQuestionDataUpdater>();
		}

		/// <summary>
		/// Adds a job to the batch that updates the given UserQuestionData object.
		/// </summary>
		public void AddToBatch(UserQuestionData userQuestionData)
		{
			var userQuestionDataUpdater = _userQuestionDataUpdaterImplFactory
				.GetUserQuestionDataUpdater(userQuestionData.AssignmentQuestion.Question);

			_userQuestionDataUpdaters.Add(userQuestionDataUpdater);

			userQuestionDataUpdater.AddToBatch(userQuestionData);
		}

		/// <summary>
		/// Updates all UserQuestionData objects. 
		/// </summary>
		public async Task UpdateAllAsync()
		{
			foreach (var userQuestionDataUpdater in _userQuestionDataUpdaters)
			{
				await userQuestionDataUpdater.UpdateAllAsync();
			}
		}
	}
}
