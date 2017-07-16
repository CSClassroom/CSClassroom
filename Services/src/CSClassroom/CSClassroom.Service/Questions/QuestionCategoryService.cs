using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions
{
	/// <summary>
	/// Performs question category operations.
	/// </summary>
	public class QuestionCategoryService : IQuestionCategoryService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionCategoryService(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns the list of question categories.
		/// </summary>
		public async Task<IList<QuestionCategory>> GetQuestionCategoriesAsync(
			string classroomName)
		{
			await LoadClassroomAsync(classroomName);

			return await _dbContext.QuestionCategories
				.Where(questionCategory => questionCategory.Classroom.Name == classroomName)
				.Where(questionCategory => questionCategory.RandomlySelectedQuestionId == null)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the question category with the given ID.
		/// </summary>
		public async Task<QuestionCategory> GetQuestionCategoryAsync(
			string classroomName, 
			int id)
		{
			await LoadClassroomAsync(classroomName);

			return await _dbContext.QuestionCategories
				.Where(questionCategory => questionCategory.Classroom.Name == classroomName)
				.SingleOrDefaultAsync(category => category.Id == id);
		}

		/// <summary>
		/// Creates a question category.
		/// </summary>
		public async Task CreateQuestionCategoryAsync(
			string classroomName, 
			QuestionCategory questionCategory)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			questionCategory.ClassroomId = classroom.Id;
			_dbContext.Add(questionCategory);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a question category.
		/// </summary>
		public async Task UpdateQuestionCategoryAsync(
			string classroomName,
			QuestionCategory questionCategory)
		{
			var currentQuestionCategory = await GetQuestionCategoryAsync
			(
				classroomName, 
				questionCategory.Id
			);

			_dbContext.Entry(currentQuestionCategory).State = EntityState.Detached;

			if (currentQuestionCategory.RandomlySelectedQuestionId != null)
			{
				throw new InvalidOperationException
				(
					"Cannot update category containing choices for a randomly selected question."
				);
			}

			questionCategory.ClassroomId = currentQuestionCategory.ClassroomId;
			_dbContext.Update(questionCategory);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes a question category.
		/// </summary>
		public async Task DeleteQuestionCategoryAsync(
			string classroomName,
			int id)
		{
			var questionCategory = await GetQuestionCategoryAsync(classroomName, id);
			_dbContext.QuestionCategories.Remove(questionCategory);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		private async Task<Classroom> LoadClassroomAsync(string classroomName)
		{
			return await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.SingleOrDefaultAsync();
		}
	}
}
