using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A question that is randomly selected from a list of questions.
	/// </summary>
	public class RandomlySelectedQuestion : Question
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RandomlySelectedQuestion()
		{
			Description = "Randomly Selected Question";
		}

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public override string QuestionTypeDisplay => "Randomly Selected Question";

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public override string QuestionTypeDescription =>
			"A question that randomly selects one question from a set of non-code questions. ";

		/// <summary>
		/// The type of solver(s) supported for this question.
		/// </summary>
		protected override QuestionSolverType SolverTypes
			=> QuestionSolverType.NonInteractive;

		/// <summary>
		/// Returns whether or not the question is a randomly selected question.
		/// </summary>
		public override bool HasChoices => true;

		/// <summary>
		/// Returns whether or not the question is a generated question template.
		/// </summary>
		public override bool IsQuestionTemplate => false;

		/// <summary>
		/// Returns whether or not this question can be duplicated.
		/// </summary>
		public override bool CanDuplicate => false;

		/// <summary>
		/// Returns whether or not this question can be turned into a generated question template.
		/// </summary>
		public override bool CanRandomize => false;

		/// <summary>
		/// The category containing the choices for this question.
		/// </summary>
		public QuestionCategory ChoicesCategory { get; set; }

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
