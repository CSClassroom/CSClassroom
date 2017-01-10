using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions.QuestionDuplicators
{
	/// <summary>
	/// Duplicates a class question.
	/// </summary>
	public class ClassQuestionDuplicator : CodeQuestionDuplicator<ClassQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassQuestionDuplicator(DatabaseContext dbContext, ClassQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected override void PopulateCodeQuestionImpl(ClassQuestion duplicate)
		{
			duplicate.Tests = DuplicateList
			(
				Question.Tests,
				obj => obj.Id = 0
			);

			duplicate.RequiredMethods = DuplicateList
			(
				Question.RequiredMethods,
				obj => obj.Id = 0
			);
		}
	}
}
