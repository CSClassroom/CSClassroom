using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Updates a randomly selected question.
	/// </summary>
	public class RandomlySelectedQuestionUpdater : QuestionUpdater<RandomlySelectedQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RandomlySelectedQuestionUpdater(
			DatabaseContext dbContext,
			RandomlySelectedQuestion question, 
			IModelErrorCollection errors) : base(dbContext, question, errors)
		{
		}

		/// <summary>
		/// Performs question-type-specific update operations.
		/// </summary>
		protected override async Task UpdateQuestionImplAsync()
		{
			if (Question.Id == 0)
			{
				var existingCategory = await DbContext.QuestionCategories
					.Where(qc => qc.Id == Question.QuestionCategoryId)
					.SingleAsync();

				var choicesCategory = new QuestionCategory()
				{
					ClassroomId = existingCategory.ClassroomId,
					Name = $"{existingCategory.Name}: {Question.Name}",
					RandomlySelectedQuestion = Question
				};

				Question.ChoicesCategory = choicesCategory;

				DbContext.QuestionCategories.Add(choicesCategory);

				Question.Description = "Randomly Selected Question";
			}
		}
	}
}
