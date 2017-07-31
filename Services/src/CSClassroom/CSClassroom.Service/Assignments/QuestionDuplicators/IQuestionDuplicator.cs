using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.QuestionDuplicators
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
