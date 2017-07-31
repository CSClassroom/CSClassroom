using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Updates the UserQuestionData objects for one or more questions
	/// </summary>
	public interface IUserQuestionDataUpdater
	{
		/// <summary>
		/// Adds a job to the batch that updates the given UserQuestionData object.
		/// </summary>
		void AddToBatch(UserQuestionData userQuestionData);

		/// <summary>
		/// Updates all UserQuestionData objects. 
		/// </summary>
		Task UpdateAllAsync();
	}
}
