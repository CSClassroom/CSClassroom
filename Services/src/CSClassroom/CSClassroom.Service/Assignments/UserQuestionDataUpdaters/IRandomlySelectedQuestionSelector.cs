using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Generates the next question to show for a randomly selected question.
	/// </summary>
	public interface IRandomlySelectedQuestionSelector
	{
		/// <summary>
		/// Returns the next question ID to use.
		/// </summary>
		int GetNextQuestionId(
			UserQuestionData userQuestionData,
			IList<int> availableQuestionIds);
	}
}
