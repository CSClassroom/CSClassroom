using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders
{
	/// <summary>
	/// Loads user question data from the database.
	/// </summary>
	public interface IUserQuestionDataLoader
	{
		/// <summary>
		/// Loads the user question data corresponding to this loader.
		/// </summary>
		Task<UserQuestionDataStore> LoadUserQuestionDataAsync();
	}
}
