using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Assignments.QuestionUpdaters
{
	/// <summary>
	/// Updates a question in the database.
	/// </summary>
	public interface IQuestionUpdater
	{
		/// <summary>
		/// Updates any related properties of the question in the database.
		/// </summary>
		Task UpdateQuestionAsync();
	}
}
