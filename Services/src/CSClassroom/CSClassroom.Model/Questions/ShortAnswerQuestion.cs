using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// An exercise with one or more blanks to fill in.
	/// </summary>
	public class ShortAnswerQuestion : Question
	{
		/// <summary>
		/// The blanks for the short answer question.
		/// </summary>
		[Display
		(
			Name = "Blanks",
			Description = "Enter the blanks for the question."
		)]
		public List<ShortAnswerQuestionBlank> Blanks { get; set; }

		/// <summary>
		/// An explanation for the answer.
		/// </summary>
		[Display
		(
			Name = "Explanation",
			Description = "Enter an explanation for the answer."
		)]
		public string Explanation { get; set; }

		/// <summary>
		/// The type of solver(s) supported for this question.
		/// </summary>
		protected override QuestionSolverType SolverTypes
			=> QuestionSolverType.Interactive | QuestionSolverType.NonInteractive;

		/// <summary>
		/// Returns whether or not the question is a randomly selected question.
		/// </summary>
		public override bool HasChoices => false;

		/// <summary>
		/// Returns whether or not the question is a generated question template.
		/// </summary>
		public override bool IsQuestionTemplate => false;

		/// <summary>
		/// Returns whether or not this question can be duplicated.
		/// </summary>
		public override bool CanDuplicate => true;

		/// <summary>
		/// Returns whether or not this question can be turned into a generated question template.
		/// </summary>
		public override bool CanRandomize => true;

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public override string QuestionTypeDisplay => "Short Answer Question";

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public override string QuestionTypeDescription =>
			"A question that has multiple blanks for short answers.";

		/// <summary>
		/// Accepts a question visitor.
		/// </summary>
		public override void Accept(IQuestionVisitor questionVisitor)
		{
			questionVisitor.Visit(this);
		}

		/// <summary>
		/// Accepts a question visitor that returns a result.
		/// </summary>
		public override TResult Accept<TResult>(IQuestionResultVisitor<TResult> questionVisitor)
		{
			return questionVisitor.Visit(this);
		}

		/// <summary>
		/// Accepts a question visitor that returns a result.
		/// </summary>
		public override TResult Accept<TResult, TParam1>(
			IQuestionResultVisitor<TResult, TParam1> questionVisitor,
			TParam1 param1)
		{
			return questionVisitor.Visit(this, param1);
		}
	}
}
