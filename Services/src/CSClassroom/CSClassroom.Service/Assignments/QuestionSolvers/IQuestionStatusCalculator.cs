using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.Service.Assignments.QuestionSolvers
{
	/// <summary>
	/// Returns the status of a question for a user.
	/// </summary>
	public interface IQuestionStatusCalculator
	{
		/// <summary>
		/// Returns information about the user's attempts for a given question,
		/// and whether future attempts are permitted. 
		/// </summary>
		UserQuestionStatus GetQuestionStatus(UserQuestionData userQuestionData);
	}
}
