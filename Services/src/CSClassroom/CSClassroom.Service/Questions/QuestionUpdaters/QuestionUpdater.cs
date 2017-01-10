using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Ensures that all relevant properties are loaded from the database
	/// for a given question.
	/// </summary>
	public abstract class QuestionUpdater<TQuestion> : IQuestionUpdater where TQuestion : Question
	{
		/// <summary>
		/// The database context.
		/// </summary>
		protected DatabaseContext DbContext { get; }

		/// <summary>
		/// The question to update.
		/// </summary>
		protected TQuestion Question { get; }

		/// <summary>
		/// The model error collection.
		/// </summary>
		protected IModelErrorCollection Errors { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected QuestionUpdater(DatabaseContext dbContext, TQuestion question, IModelErrorCollection errors)
		{
			DbContext = dbContext;
			Question = question;
			Errors = errors;
		}

		/// <summary>
		/// Updates any related properties of the question in the database.
		/// </summary>
		public async Task UpdateQuestionAsync()
		{
			UpdatePrerequisiteQuestions(Question.PrerequisiteQuestions);

			DbContext.RemoveUnwantedObjects
			(
				DbContext.PrerequisiteQuestions,
				pq => pq.Id,
				pq => pq.SecondQuestionId == Question.Id,
				Question.PrerequisiteQuestions
			);

			await UpdateQuestionImplAsync();
		}

		/// <summary>
		/// Updates any subclass-specific related properties of the question in the database.
		/// </summary>
		protected abstract Task UpdateQuestionImplAsync();

		/// <summary>
		/// Updates each code constraint.
		/// </summary>
		private void UpdatePrerequisiteQuestions(IEnumerable<PrerequisiteQuestion> prereqs)
		{
			if (prereqs != null)
			{
				int order = 0;
				foreach (var prereq in prereqs)
				{
					prereq.Order = order;
					prereq.SecondQuestionId = Question.Id;
					order++;
				}
			}
		}
	}
}
