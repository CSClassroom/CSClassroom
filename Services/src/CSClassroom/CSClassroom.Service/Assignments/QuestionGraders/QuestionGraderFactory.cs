using System;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionGraders
{
	/// <summary>
	/// Creates question graders.
	/// </summary>
	public class QuestionGraderFactory :
		IQuestionGraderFactory,
		IQuestionResultVisitor<IQuestionGrader>
	{
		/// <summary>
		/// The code runner service.
		/// </summary>
		private readonly ICodeRunnerService _codeRunnerService;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionGraderFactory(ICodeRunnerService codeRunnerService)
		{
			_codeRunnerService = codeRunnerService;
		}

		/// <summary>
		/// Creates a question grader.
		/// </summary>
		public virtual IQuestionGrader CreateQuestionGrader(Question question)
		{
			return question.Accept(this);
		}

		/// <summary>
		/// Creates a question grader for a multiple choice question.
		/// </summary>
		IQuestionGrader IQuestionResultVisitor<IQuestionGrader>.Visit(MultipleChoiceQuestion question)
		{
			return new MultipleChoiceQuestionGrader(question);
		}

		/// <summary>
		/// Creates a question grader for a short answer question.
		/// </summary>
		IQuestionGrader IQuestionResultVisitor<IQuestionGrader>.Visit(ShortAnswerQuestion question)
		{
			return new ShortAnswerQuestionGrader(question);
		}

		/// <summary>
		/// Creates a question grader for a method question.
		/// </summary>
		IQuestionGrader IQuestionResultVisitor<IQuestionGrader>.Visit(MethodQuestion question)
		{
			return new MethodQuestionGrader(question, _codeRunnerService);
		}

		/// <summary>
		/// Creates a question grader for a class question.
		/// </summary>
		IQuestionGrader IQuestionResultVisitor<IQuestionGrader>.Visit(ClassQuestion question)
		{
			return new ClassQuestionGrader(question, _codeRunnerService);
		}

		/// <summary>
		/// Creates a question grader for a class question.
		/// </summary>
		IQuestionGrader IQuestionResultVisitor<IQuestionGrader>.Visit(ProgramQuestion question)
		{
			return new ProgramQuestionGrader(question, _codeRunnerService);
		}

		/// <summary>
		/// Creates a question loader for a generated question.
		/// </summary>
		IQuestionGrader IQuestionResultVisitor<IQuestionGrader>.Visit(GeneratedQuestionTemplate question)
		{
			throw new InvalidOperationException("Generated questions cannot be graded directly. Instead, grade the question with a particular seed.");
		}

		/// <summary>
		/// Creates a question loader for a randomly selected question.
		/// </summary>
		IQuestionGrader IQuestionResultVisitor<IQuestionGrader>.Visit(RandomlySelectedQuestion question)
		{
			throw new InvalidOperationException("Randomly-selected questions cannot be graded directly. Instead, grade actual question selected.");
		}
	}
}
