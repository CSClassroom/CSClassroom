using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// Creates user question data updaters.
	/// </summary>
	public interface IUserQuestionDataUpdaterFactory
	{
		/// <summary>
		/// Creates a new UserQuestionDataUpdater.
		/// </summary>
		IUserQuestionDataUpdater CreateUserQuestionDataUpdater();
	}
}
