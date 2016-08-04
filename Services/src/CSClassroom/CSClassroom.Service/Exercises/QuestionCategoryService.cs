using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Exercises;
using CSC.CSClassroom.Service.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Exercises
{
	/// <summary>
	/// Performs question category operations.
	/// </summary>
	public class QuestionCategoryService : IQuestionCategoryService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private DatabaseContext _dbContext;

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
		public async Task<IList<QuestionCategory>> GetQuestionCategoriesAsync(Group group)
		{
			return await _dbContext.QuestionCategories
				.Where(questionCategory => questionCategory.GroupId == group.Id)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the question category with the given ID.
		/// </summary>
		public async Task<QuestionCategory> GetQuestionCategoryAsync(Group group, int id)
		{
			return await _dbContext.QuestionCategories
				.Where(questionCategory => questionCategory.GroupId == group.Id)
				.SingleOrDefaultAsync(category => category.Id == id);
		}

		/// <summary>
		/// Creates a question category.
		/// </summary>
		public async Task CreateQuestionCategoryAsync(Group group, QuestionCategory questionCategory)
		{
			questionCategory.GroupId = group.Id;
			_dbContext.Add(questionCategory);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a question category.
		/// </summary>
		public async Task UpdateQuestionCategoryAsync(Group group, QuestionCategory questionCategory)
		{
			questionCategory.GroupId = group.Id;
			_dbContext.Update(questionCategory);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes a question category.
		/// </summary>
		public async Task DeleteQuestionCategoryAsync(Group group, int id)
		{
			var questionCategory = await GetQuestionCategoryAsync(group, id);
			_dbContext.QuestionCategories.Remove(questionCategory);

			await _dbContext.SaveChangesAsync();
		}
	}
}
