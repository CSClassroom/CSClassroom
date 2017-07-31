using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionLoaders
{
	/// <summary>
	/// Creates question loaders.
	/// </summary>
	public class QuestionLoaderFactory : 
		IQuestionLoaderFactory, 
		IQuestionResultVisitor<IQuestionLoader>
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionLoaderFactory(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Creates a question loader.
		/// </summary>
		public virtual IQuestionLoader CreateQuestionLoader(Question question)
		{
			return question.Accept(this);
		}

		/// <summary>
		/// Creates a question loader for a multiple choice question.
		/// </summary>
		IQuestionLoader IQuestionResultVisitor<IQuestionLoader>.Visit(MultipleChoiceQuestion question)
		{
			return new MultipleChoiceQuestionLoader(_dbContext, question);
		}

		/// <summary>
		/// Creates a question loader for a short answer question.
		/// </summary>
		IQuestionLoader IQuestionResultVisitor<IQuestionLoader>.Visit(ShortAnswerQuestion question)
		{
			return new ShortAnswerQuestionLoader(_dbContext, question);
		}

		/// <summary>
		/// Creates a question loader for a method question.
		/// </summary>
		IQuestionLoader IQuestionResultVisitor<IQuestionLoader>.Visit(MethodQuestion question)
		{
			return new MethodQuestionLoader(_dbContext, question);
		}

		/// <summary>
		/// Creates a question loader for a class question.
		/// </summary>
		IQuestionLoader IQuestionResultVisitor<IQuestionLoader>.Visit(ClassQuestion question)
		{
			return new ClassQuestionLoader(_dbContext, question);
		}

		/// <summary>
		/// Creates a question loader for a program question.
		/// </summary>
		IQuestionLoader IQuestionResultVisitor<IQuestionLoader>.Visit(ProgramQuestion question)
		{
			return new ProgramQuestionLoader(_dbContext, question);
		}

		/// <summary>
		/// Creates a question loader for a generated question.
		/// </summary>
		IQuestionLoader IQuestionResultVisitor<IQuestionLoader>.Visit(GeneratedQuestionTemplate question)
		{
			return new GeneratedQuestionLoader(_dbContext, question);
		}

		/// <summary>
		/// Creates a question loader for a randomly selected question.
		/// </summary>
		IQuestionLoader IQuestionResultVisitor<IQuestionLoader>.Visit(RandomlySelectedQuestion question)
		{
			return new RandomlySelectedQuestionLoader(_dbContext, question);
		}
	}
}
