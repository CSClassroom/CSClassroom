using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Creates UserQuestionDataUpdater implementations for a given UserQuestionData object.
	/// </summary>
	public interface IUserQuestionDataUpdaterImplFactory
	{
		/// <summary>
		/// Returns the user question updater for the given question type.
		/// </summary>
		IUserQuestionDataUpdater GetUserQuestionDataUpdater(Question question);
	}
}
