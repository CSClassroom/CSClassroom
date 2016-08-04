using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Exercises;
using CSC.CSClassroom.Service.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Exercises
{
	/// <summary>
	/// Performs question operations.
	/// </summary>
	public class QuestionService : IQuestionService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionService(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns the list of questions.
		/// </summary>
		public async Task<IList<Question>> GetQuestionsAsync(Group group)
		{
			return await _dbContext.Questions
				.Where(question => question.QuestionCategory.GroupId == group.Id)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the question with the given ID.
		/// </summary>
		public async Task<Question> GetQuestionAsync(Group group, int id)
		{
			return await _dbContext.Questions
				.Where(question => question.QuestionCategory.GroupId == group.Id)
				.SingleOrDefaultAsync(question => question.Id == id);
		}

		/// <summary>
		/// Creates a question.
		/// </summary>
		public async Task CreateQuestionAsync(Group group, Question question)
		{
			var questionCategory = await _dbContext.QuestionCategories
				.SingleOrDefaultAsync(category => category.Id == question.QuestionCategoryId);

			if (questionCategory.GroupId != group.Id)
				throw new InvalidOperationException("Category of question is not in the given group.");

			_dbContext.Add(question);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a question.
		/// </summary>
		public async Task UpdateQuestionAsync(Group group, Question question)
		{
			var questionCategory = await _dbContext.QuestionCategories
				.SingleOrDefaultAsync(category => category.Id == question.QuestionCategoryId);

			if (questionCategory.GroupId != group.Id)
				throw new InvalidOperationException("Category of question is not in the given group.");

			_dbContext.Update(question);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes a question.
		/// </summary>
		public async Task DeleteQuestionAsync(Group group, int id)
		{
			var question = await GetQuestionAsync(group, id);
			_dbContext.Questions.Remove(question);

			await _dbContext.SaveChangesAsync();
		}
	}
}
