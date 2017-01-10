using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionDuplicators
{
	/// <summary>
	/// Duplicates question objects.
	/// </summary>
	public interface IQuestionDuplicator
	{
		/// <summary>
		/// Returns a duplicate question object.
		/// </summary>
		Question DuplicateQuestion();
	}
}
