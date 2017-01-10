using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Updates a multiple choice question.
	/// </summary>
	public class MultipleChoiceQuestionUpdater : QuestionUpdater<MultipleChoiceQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MultipleChoiceQuestionUpdater(
			DatabaseContext dbContext, 
			MultipleChoiceQuestion question, 
			IModelErrorCollection errors) : base(dbContext, question, errors)
		{
		}

		/// <summary>
		/// Performs question-type-specific update operations.
		/// </summary>
		protected override Task UpdateQuestionImplAsync()
		{
			UpdateChoices(Question.Choices);

			DbContext.RemoveUnwantedObjects
			(
				DbContext.MultipleChoiceQuestionChoices,
				choice => choice.Id,
				choice => choice.MultipleChoiceQuestionId == Question.Id,
				Question.Choices
			);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Updates each choice.
		/// </summary>
		private static void UpdateChoices(IEnumerable<MultipleChoiceQuestionChoice> choices)
		{
			int order = 0;
			foreach (var choice in choices)
			{
				choice.Value = choice.Value?.Replace("\r\n", "\n");
				choice.Order = order;
				order++;
			}
		}
	}
}
