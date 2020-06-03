using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.Validators
{
	/// <summary>
	/// Validates add/update operations for questions.
	/// </summary>
	public class QuestionValidator : IQuestionValidator
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionValidator(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Ensures that a question to add or update is in a valid state.
		/// </summary>
		public async Task<bool> ValidateQuestionAsync(
			Question question,
			IModelErrorCollection errors,
			string classroomName)
		{
			var existingQuestion = await _dbContext.Questions
				.Where(q => q.QuestionCategory.Classroom.Name == classroomName)
				.Where(q => q.Id == question.Id)
				.Include(q => q.QuestionCategory)
				.SingleOrDefaultAsync();

			if (existingQuestion != null)
			{
				_dbContext.Entry(existingQuestion).State = EntityState.Detached;
			}

			var newQuestionCategory = await _dbContext.QuestionCategories
				.Include(qc => qc.Classroom)
				.SingleOrDefaultAsync
				(
					category => category.Id == question.QuestionCategoryId
				);

			if (newQuestionCategory.Classroom.Name != classroomName)
			{
				throw new InvalidOperationException(
					"Category of question is not in the given classroom.");
			}

			if (existingQuestion?.QuestionCategory?.RandomlySelectedQuestionId != null
				&& question.QuestionCategoryId != existingQuestion?.QuestionCategoryId)
			{
				throw new InvalidOperationException
				(
					"The category cannot be changed for a randomly selected question choice."
				);
			}

			if (existingQuestion != null
				&& existingQuestion?.QuestionCategory?.RandomlySelectedQuestionId == null
				&& newQuestionCategory.RandomlySelectedQuestionId != null)
			{
				throw new InvalidOperationException
				(
					"The category cannot be changed from a non-random-choice category "
					+ "to a random choice category."
				);
			}

			if (await _dbContext.Questions.AnyAsync(
				q => q.Id != question.Id
					 && q.Name == question.Name
					 && q.QuestionCategoryId == question.QuestionCategoryId))
			{
				errors.AddError("Name", "Another question with that name already exists.");
			}

			return !errors.HasErrors;
		}
	}
}
