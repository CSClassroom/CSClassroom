using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.QuestionDuplicators
{
	/// <summary>
	/// Duplicates a method question.
	/// </summary>
	public class MethodQuestionDuplicator : CodeQuestionDuplicator<MethodQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodQuestionDuplicator(DatabaseContext dbContext, MethodQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected override void PopulateCodeQuestionImpl(MethodQuestion duplicate)
		{
			duplicate.Tests = DuplicateList
			(
				Question.Tests, 
				obj => obj.Id = 0
			);
		}
	}
}
