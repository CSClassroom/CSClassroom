using System;
using System.Collections.Generic;
using System.Text;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;

namespace CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// Creates user question data updaters.
	/// </summary>
	public class UserQuestionDataUpdaterFactory : IUserQuestionDataUpdaterFactory
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Generates questions from a generated question template.
		/// </summary>
		private readonly IQuestionGenerator _questionGenerator;

		/// <summary>
		/// Generates new seeds for generated question templates.
		/// </summary>
		private readonly IGeneratedQuestionSeedGenerator _seedGenerator;

		/// <summary>
		/// Selects new questions for randomly selected questions.
		/// </summary>
		private readonly IRandomlySelectedQuestionSelector _questionSelector;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserQuestionDataUpdaterFactory(
			DatabaseContext dbContext,
			IQuestionGenerator questionGenerator,
			IGeneratedQuestionSeedGenerator seedGenerator,
			IRandomlySelectedQuestionSelector questionSelector,
			ITimeProvider timeProvider)
		{
			_dbContext = dbContext;
			_questionGenerator = questionGenerator;
			_seedGenerator = seedGenerator;
			_questionSelector = questionSelector;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Creates a new UserQuestionDataUpdater.
		/// </summary>
		public virtual IUserQuestionDataUpdater CreateUserQuestionDataUpdater()
		{
			return new AggregateUserQuestionDataUpdater
			(
				new UserQuestionDataUpdaterImplFactory
				(
					_dbContext, 
					_questionGenerator,
					_seedGenerator,
					_questionSelector, 
					_timeProvider
				)
			);
		}
	}
}
