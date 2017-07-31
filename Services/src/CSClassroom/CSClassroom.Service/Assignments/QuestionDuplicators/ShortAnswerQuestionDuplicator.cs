using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.QuestionDuplicators
{
	/// <summary>
	/// Loads a short answer question.
	/// </summary>
	public class ShortAnswerQuestionDuplicator : QuestionDuplicator<ShortAnswerQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ShortAnswerQuestionDuplicator(DatabaseContext dbContext, ShortAnswerQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected override void PopulateQuestionImpl(ShortAnswerQuestion duplicate)
		{
			duplicate.Blanks = DuplicateList
			(
				Question.Blanks,
				obj => obj.Id = 0
			);
		}
	}
}
