using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.QuestionDuplicators
{
	/// <summary>
	/// Detaches a multiple choice question.
	/// </summary>
	public class MultipleChoiceQuestionDuplicator : QuestionDuplicator<MultipleChoiceQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MultipleChoiceQuestionDuplicator(DatabaseContext dbContext, MultipleChoiceQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected override void PopulateQuestionImpl(MultipleChoiceQuestion duplicate)
		{
			duplicate.Choices = DuplicateList
			(
				Question.Choices,
				obj => obj.Id = 0
			);
		}
	}
}
