using System;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionDuplicators
{
	/// <summary>
	/// Creates question duplicators.
	/// </summary>
	public class QuestionDuplicatorFactory :
		IQuestionDuplicatorFactory, 
		IQuestionResultVisitor<IQuestionDuplicator>
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionDuplicatorFactory(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Creates a question duplicator.
		/// </summary>
		public virtual IQuestionDuplicator CreateQuestionDuplicator(Question question)
		{
			return question.Accept(this);
		}

		/// <summary>
		/// Creates a question duplicator for a multiple choice question.
		/// </summary>
		IQuestionDuplicator IQuestionResultVisitor<IQuestionDuplicator>.Visit(MultipleChoiceQuestion question)
		{
			return new MultipleChoiceQuestionDuplicator(_dbContext, question);
		}

		/// <summary>
		/// Creates a question duplicator for a short answer question.
		/// </summary>
		IQuestionDuplicator IQuestionResultVisitor<IQuestionDuplicator>.Visit(ShortAnswerQuestion question)
		{
			return new ShortAnswerQuestionDuplicator(_dbContext, question);
		}

		/// <summary>
		/// Creates a question duplicator for a method question.
		/// </summary>
		IQuestionDuplicator IQuestionResultVisitor<IQuestionDuplicator>.Visit(MethodQuestion question)
		{
			return new MethodQuestionDuplicator(_dbContext, question);
		}

		/// <summary>
		/// Creates a question duplicator for a class question.
		/// </summary>
		IQuestionDuplicator IQuestionResultVisitor<IQuestionDuplicator>.Visit(ClassQuestion question)
		{
			return new ClassQuestionDuplicator(_dbContext, question);
		}

		/// <summary>
		/// Creates a question duplicator for a program question.
		/// </summary>
		IQuestionDuplicator IQuestionResultVisitor<IQuestionDuplicator>.Visit(ProgramQuestion question)
		{
			return new ProgramQuestionDuplicator(_dbContext, question);
		}

		/// <summary>
		/// Creates a question duplicator for a generated question.
		/// </summary>
		IQuestionDuplicator IQuestionResultVisitor<IQuestionDuplicator>.Visit(GeneratedQuestionTemplate question)
		{
			return new GeneratedQuestionDuplicator(_dbContext, question);
		}

		/// <summary>
		/// Creates a question duplicator for a randomly selected question.
		/// </summary>
		IQuestionDuplicator IQuestionResultVisitor<IQuestionDuplicator>.Visit(RandomlySelectedQuestion question)
		{
			throw new InvalidOperationException("Randomly selected questions cannot be duplicated.");
		}
	}
}
