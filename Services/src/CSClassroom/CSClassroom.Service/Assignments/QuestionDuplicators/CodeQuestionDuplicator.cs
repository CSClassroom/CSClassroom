using System.Collections.Generic;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions.QuestionDuplicators
{
	/// <summary>
	/// Duplicates a code question.
	/// </summary>
	public abstract class CodeQuestionDuplicator<TQuestion> : QuestionDuplicator<TQuestion> where TQuestion : CodeQuestion
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		protected CodeQuestionDuplicator(DatabaseContext dbContext, TQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected sealed override void PopulateQuestionImpl(TQuestion duplicate)
		{
			duplicate.ImportedClasses = DuplicateList
			(
				Question.ImportedClasses, 
				obj => obj.Id = 0
			);

			duplicate.CodeConstraints = DuplicateList
			(
				Question.CodeConstraints,
				obj => obj.Id = 0
			);

			PopulateCodeQuestionImpl(duplicate);
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected abstract void PopulateCodeQuestionImpl(TQuestion duplicate);
	}
}
