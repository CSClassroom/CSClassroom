using System;
using System.Collections.Generic;
using System.Text;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Creates UserQuestionDataUpdater implementations for a given UserQuestionData object.
	/// </summary>
	public class UserQuestionDataUpdaterImplFactory :
		IUserQuestionDataUpdaterImplFactory,
		IQuestionResultVisitor<IUserQuestionDataUpdater>
	{
		/// <summary>
		/// A user question data updater for generated question templates.
		/// </summary>
		private readonly GeneratedUserQuestionDataUpdater _generatedUserQuestionDataUpdater;

		/// <summary>
		/// A user question data updater for randomly selected questions.
		/// </summary>
		private readonly RandomlySelectedUserQuestionDataUpdater _randomlySelectedUserQuestionDataUpdater;

		/// <summary>
		/// A user question data updater for all other question types.
		/// </summary>
		private readonly DefaultUserQuestionDataUpdater _defaultUserQuestionDataUpdater;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserQuestionDataUpdaterImplFactory(
			DatabaseContext dbContext,
			IQuestionGenerator questionGenerator,
			IGeneratedQuestionSeedGenerator seedGenerator,
			IRandomlySelectedQuestionSelector questionSelector,
			ITimeProvider timeProvider)
		{
			_generatedUserQuestionDataUpdater = new GeneratedUserQuestionDataUpdater
			(
				questionGenerator,
				seedGenerator,
				timeProvider
			);

			_randomlySelectedUserQuestionDataUpdater = new RandomlySelectedUserQuestionDataUpdater
			(
				dbContext,
				questionSelector
			);

			_defaultUserQuestionDataUpdater = new DefaultUserQuestionDataUpdater();
		}

		/// <summary>
		/// Returns the user question updater for the given question type.
		/// </summary>
		public IUserQuestionDataUpdater GetUserQuestionDataUpdater(Question question)
		{
			return question.Accept(this);
		}

		/// <summary>
		/// Creates a question loader for a multiple choice question.
		/// </summary>
		IUserQuestionDataUpdater IQuestionResultVisitor<IUserQuestionDataUpdater>.Visit(
			MultipleChoiceQuestion question)
		{
			return _defaultUserQuestionDataUpdater;
		}

		/// <summary>
		/// Creates a question loader for a short answer question.
		/// </summary>
		IUserQuestionDataUpdater IQuestionResultVisitor<IUserQuestionDataUpdater>.Visit(
			ShortAnswerQuestion question)
		{
			return _defaultUserQuestionDataUpdater;
		}

		/// <summary>
		/// Creates a question loader for a method question.
		/// </summary>
		IUserQuestionDataUpdater IQuestionResultVisitor<IUserQuestionDataUpdater>.Visit(
			MethodQuestion question)
		{
			return _defaultUserQuestionDataUpdater;
		}

		/// <summary>
		/// Creates a question loader for a class question.
		/// </summary>
		IUserQuestionDataUpdater IQuestionResultVisitor<IUserQuestionDataUpdater>.Visit(
			ClassQuestion question)
		{
			return _defaultUserQuestionDataUpdater;
		}

		/// <summary>
		/// Creates a question loader for a program question.
		/// </summary>
		IUserQuestionDataUpdater IQuestionResultVisitor<IUserQuestionDataUpdater>.Visit(
			ProgramQuestion question)
		{
			return _defaultUserQuestionDataUpdater;
		}

		/// <summary>
		/// Creates a question loader for a generated question.
		/// </summary>
		IUserQuestionDataUpdater IQuestionResultVisitor<IUserQuestionDataUpdater>.Visit(
			GeneratedQuestionTemplate question)
		{
			return _generatedUserQuestionDataUpdater;
		}

		/// <summary>
		/// Creates a question loader for a randomly selected question.
		/// </summary>
		IUserQuestionDataUpdater IQuestionResultVisitor<IUserQuestionDataUpdater>.Visit(
			RandomlySelectedQuestion question)
		{
			return _randomlySelectedUserQuestionDataUpdater;
		}
	}
}
