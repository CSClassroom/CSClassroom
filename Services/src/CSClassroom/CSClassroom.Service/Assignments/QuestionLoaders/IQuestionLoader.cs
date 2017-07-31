using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Assignments.QuestionLoaders
{
	/// <summary>
	/// Loads a question from the database.
	/// </summary>
	public interface IQuestionLoader
	{
		/// <summary>
		/// Loads any related properties of the question from the database.
		/// </summary>
		Task LoadQuestionAsync();
	}
}
