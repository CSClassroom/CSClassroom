using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionUpdaters
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
			await UpdateQuestionImplAsync();
		}

		/// <summary>
		/// Updates any subclass-specific related properties of the question in the database.
		/// </summary>
		protected abstract Task UpdateQuestionImplAsync();
	}
}
