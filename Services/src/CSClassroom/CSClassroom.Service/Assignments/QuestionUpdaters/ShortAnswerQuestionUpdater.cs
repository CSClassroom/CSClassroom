using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Updates a short answer question.
	/// </summary>
	public class ShortAnswerQuestionUpdater : QuestionUpdater<ShortAnswerQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ShortAnswerQuestionUpdater(
			DatabaseContext dbContext,
			ShortAnswerQuestion question, 
			IModelErrorCollection errors) : base(dbContext, question, errors)
		{
		}

		/// <summary>
		/// Performs question-type-specific update operations.
		/// </summary>
		protected override Task UpdateQuestionImplAsync()
		{
			UpdateBlanks(Question.Blanks);

			DbContext.RemoveUnwantedObjects
			(
				DbContext.ShortAnswerQuestionBlanks,
				choice => choice.Id,
				choice => choice.ShortAnswerQuestionId == Question.Id,
				Question.Blanks
			);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Updates each blank.
		/// </summary>
		private static void UpdateBlanks(IEnumerable<ShortAnswerQuestionBlank> blanks)
		{
			int order = 0;
			foreach (var blank in blanks)
			{
				blank.Name = blank.Name?.Replace("\r\n", "\n");
				blank.Answer = blank.Answer?.Replace("\r\n", "\n");
				blank.Order = order;
				order++;
			}
		}
	}
}
