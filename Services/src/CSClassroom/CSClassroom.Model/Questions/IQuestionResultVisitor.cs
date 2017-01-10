namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A question visitor that returns a result.
	/// </summary>
	public interface IQuestionResultVisitor<TResult>
	{
		/// <summary>
		/// Visits a multiple choice question.
		/// </summary>
		TResult Visit(MultipleChoiceQuestion question);

		/// <summary>
		/// Visits a short answer question.
		/// </summary>
		TResult Visit(ShortAnswerQuestion question);

		/// <summary>
		/// Visits a method question.
		/// </summary>
		TResult Visit(MethodQuestion question);

		/// <summary>
		/// Visits a class question.
		/// </summary>
		TResult Visit(ClassQuestion question);

		/// <summary>
		/// Visits a program question.
		/// </summary>
		TResult Visit(ProgramQuestion question);

		/// <summary>
		/// Visits a generated question.
		/// </summary>
		TResult Visit(GeneratedQuestionTemplate question);
	}

	/// <summary>
	/// A question visitor that returns a result.
	/// </summary>
	public interface IQuestionResultVisitor<TResult, TParam1>
	{
		/// <summary>
		/// Visits a multiple choice question.
		/// </summary>
		TResult Visit(MultipleChoiceQuestion question, TParam1 param1);

		/// <summary>
		/// Visits a short answer question.
		/// </summary>
		TResult Visit(ShortAnswerQuestion question, TParam1 param1);

		/// <summary>
		/// Visits a method question.
		/// </summary>
		TResult Visit(MethodQuestion question, TParam1 param1);

		/// <summary>
		/// Visits a class question.
		/// </summary>
		TResult Visit(ClassQuestion question, TParam1 param1);

		/// <summary>
		/// Visits a program question.
		/// </summary>
		TResult Visit(ProgramQuestion question, TParam1 param1);

		/// <summary>
		/// Visits a generated question.
		/// </summary>
		TResult Visit(GeneratedQuestionTemplate question, TParam1 param1);
	}
}
