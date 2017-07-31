using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// A UserQuestionDataUpdater that does nothing.
	/// </summary>
	public class DefaultUserQuestionDataUpdater : IUserQuestionDataUpdater
	{
		/// <summary>
		/// Adds a job to the batch that updates the given UserQuestionData object.
		/// </summary>
		public void AddToBatch(UserQuestionData userQuestionData)
		{
		}

		/// <summary>
		/// Updates all UserQuestionData objects. 
		/// </summary>
		public Task UpdateAllAsync()
		{
			return Task.CompletedTask;
		}
	}
}
