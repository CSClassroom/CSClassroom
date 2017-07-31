namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A question visitor.
	/// </summary>
	public interface IQuestionVisitor
	{
		/// <summary>
		/// Visits a multiple choice question.
		/// </summary>
		void Visit(MultipleChoiceQuestion question);

		/// <summary>
		/// Visits a short answer question.
		/// </summary>
		void Visit(ShortAnswerQuestion question);

		/// <summary>
		/// Visits a method question.
		/// </summary>
		void Visit(MethodQuestion question);

		/// <summary>
		/// Visits a class question.
		/// </summary>
		void Visit(ClassQuestion question);

		/// <summary>
		/// Visits a program question.
		/// </summary>
		void Visit(ProgramQuestion question);

		/// <summary>
		/// Visits a generated question.
		/// </summary>
		void Visit(GeneratedQuestionTemplate question);

		/// <summary>
		/// Visits a randomly selected question.
		/// </summary>
		void Visit(RandomlySelectedQuestion question);
	}
}
