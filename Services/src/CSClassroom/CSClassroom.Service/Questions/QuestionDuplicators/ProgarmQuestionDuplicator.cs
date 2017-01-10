using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions.QuestionDuplicators
{
	/// <summary>
	/// Duplicates a program question.
	/// </summary>
	public class ProgramQuestionDuplicator : CodeQuestionDuplicator<ProgramQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ProgramQuestionDuplicator(DatabaseContext dbContext, ProgramQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected override void PopulateCodeQuestionImpl(ProgramQuestion duplicate)
		{
			duplicate.Tests = DuplicateList
			(
				Question.Tests,
				obj => obj.Id = 0
			);
		}
	}
}
