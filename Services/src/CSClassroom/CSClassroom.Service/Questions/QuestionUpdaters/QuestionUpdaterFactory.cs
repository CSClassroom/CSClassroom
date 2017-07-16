using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Creates question updaters.
	/// </summary>
	public class QuestionUpdaterFactory : 
		IQuestionUpdaterFactory, 
		IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The question generator.
		/// </summary>
		private readonly IQuestionGenerator _questionGenerator;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionUpdaterFactory(
			DatabaseContext dbContext, 
			IQuestionGenerator questionGenerator, 
			ITimeProvider timeProvider)
		{
			_dbContext = dbContext;
			_questionGenerator = questionGenerator;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Creates a question updater.
		/// </summary>
		public virtual IQuestionUpdater CreateQuestionUpdater(Question question, IModelErrorCollection errors)
		{
			return question.Accept(this, errors);
		}

		/// <summary>
		/// Creates a question updater for a multiple choice question.
		/// </summary>
		IQuestionUpdater IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>.Visit(
			MultipleChoiceQuestion question,
			IModelErrorCollection errors)
		{
			return new MultipleChoiceQuestionUpdater(_dbContext, question, errors);
		}

		/// <summary>
		/// Creates a question updater for a short answer question.
		/// </summary>
		IQuestionUpdater IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>.Visit(
			ShortAnswerQuestion question,
			IModelErrorCollection errors)
		{
			return new ShortAnswerQuestionUpdater(_dbContext, question, errors);
		}

		/// <summary>
		/// Creates a question updater for a method question.
		/// </summary>
		IQuestionUpdater IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>.Visit(
			MethodQuestion question,
			IModelErrorCollection errors)
		{
			return new MethodQuestionUpdater(_dbContext, question, errors);
		}

		/// <summary>
		/// Creates a question updater for a class question.
		/// </summary>
		IQuestionUpdater IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>.Visit(
			ClassQuestion question, 
			IModelErrorCollection errors)
		{
			return new ClassQuestionUpdater(_dbContext, question, errors);
		}

		/// <summary>
		/// Creates a question updater for a program question.
		/// </summary>
		IQuestionUpdater IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>.Visit(
			ProgramQuestion question,
			IModelErrorCollection errors)
		{
			return new ProgramQuestionUpdater(_dbContext, question, errors);
		}

		/// <summary>
		/// Creates a question updater for a generated question.
		/// </summary>
		IQuestionUpdater IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>.Visit(
			GeneratedQuestionTemplate question,
			IModelErrorCollection errors)
		{
			return new GeneratedQuestionUpdater
			(
				_dbContext, 
				question, 
				errors, 
				_questionGenerator, 
				_timeProvider
			);
		}

		/// <summary>
		/// Creates a question updater for a randomly selected question.
		/// </summary>
		IQuestionUpdater IQuestionResultVisitor<IQuestionUpdater, IModelErrorCollection>.Visit(
			RandomlySelectedQuestion question,
			IModelErrorCollection errors)
		{
			return new RandomlySelectedQuestionUpdater(_dbContext, question, errors);
		}
	}
}
