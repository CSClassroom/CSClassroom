using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionSolvers
{
	/// <summary>
	/// Retrieves unsolved prerequisite questions from the database
	/// for a given question in an assignment.
	/// </summary>
	public interface IUnsolvedPrereqsRetriever
	{
		/// <summary>
		/// Returns a list of unsolved prerequisite question IDs.
		/// </summary>
		Task<IList<AssignmentQuestion>> GetUnsolvedPrereqsAsync(
			UserQuestionData userQuestionData);
	}
}
