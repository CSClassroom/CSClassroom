using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionLoaders
{
	/// <summary>
	/// Ensures that all relevant properties are loaded from the database
	/// for a given question.
	/// </summary>
	public abstract class QuestionLoader<TQuestion> : IQuestionLoader where TQuestion : Question
	{
		/// <summary>
		/// The database context.
		/// </summary>
		protected DatabaseContext DbContext { get; }

		/// <summary>
		/// The question to load.
		/// </summary>
		protected TQuestion Question { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected QuestionLoader(DatabaseContext dbContext, TQuestion question)
		{
			DbContext = dbContext;
			Question = question;
		}

		/// <summary>
		/// Loads any related properties of the question from the database.
		/// </summary>
		public async Task LoadQuestionAsync()
		{
			await LoadReferenceAsync(q => q.QuestionCategory);

			await LoadQuestionImplAsync();
		}

		/// <summary>
		/// Loads a single reference navigation property.
		/// </summary>
		protected async Task LoadReferenceAsync<TObjectType>(
			Expression<Func<TQuestion, TObjectType>> member)
				where TObjectType : class
		{
			await DbContext.Entry(Question)
				.Reference(member)
				.LoadAsync();
		}

		/// <summary>
		/// Loads a collection navigation property.
		/// </summary>
		protected async Task LoadCollectionAsync<TObjectType>(
			Expression<Func<TQuestion, IEnumerable<TObjectType>>> member) 
				where TObjectType : class
		{
			await DbContext.Entry(Question)
				.Collection(member)
				.LoadAsync();
		}

		/// <summary>
		/// Performs any question-type-specific loading operations.
		/// </summary>
		protected abstract Task LoadQuestionImplAsync();
	}
}
