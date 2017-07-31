using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// An exercise.
	/// </summary>
	public abstract class Question
	{
		/// <summary>
		/// The unique ID for the exercise.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the category that contains this exercise.
		/// </summary>
		[Display
		(
			Name = "Category",
			Description = "Select the category for the question."
		)]
		public int QuestionCategoryId { get; set; }

		/// <summary>
		/// The name of the exercise.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[Display
		(
			Name = "Question Name",
			Description = "Enter the name of the question."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The description of the exericse.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Description",
			Description = "Enter the description for the question, using the Markdown language."
		)]
		public string Description { get; set; }
		
		/// <summary>
		/// Allows partial credit.
		/// </summary>
		[Display
		(
			Name = "Allow Partial Credit",
			Description = "Select whether to allow partial credit."
		)]
		public bool AllowPartialCredit { get; set; }

		/// <summary>
		/// The category that contains this exercise.
		/// </summary>
		public virtual QuestionCategory QuestionCategory { get; set; }

		/// <summary>
		/// The assignment questions referring to this question. 
		/// </summary>
		public virtual ICollection<AssignmentQuestion> AssignmentQuestions { get; set; }

		/// <summary>
		/// Returns whether or not the question is a randomly selected question.
		/// </summary>
		public abstract bool HasChoices { get; }

		/// <summary>
		/// Returns whether or not the question is a generated question template.
		/// </summary>
		public abstract bool IsQuestionTemplate { get; }

		/// <summary>
		/// Returns whether or not this question can be duplicated.
		/// </summary>
		public abstract bool CanDuplicate { get; }

		/// <summary>
		/// Returns whether or not this question can be turned into a generated question template.
		/// </summary>
		public abstract bool CanRandomize { get; }

		/// <summary>
		/// The type of solver(s) supported for this question.
		/// </summary>
		protected abstract QuestionSolverType SolverTypes { get; }

		/// <summary>
		/// Returns whether or not the question supports the given solver type.
		/// </summary>
		public bool SupportedSolver(QuestionSolverType solverType)
			=> (SolverTypes & solverType) == solverType;

		/// <summary>
		/// Returns whether or not the question does not support the given solver type.
		/// </summary>
		public bool UnsupportedSolver(QuestionSolverType solverType)
			=> (SolverTypes & solverType) == 0;

		/// <summary>
		/// Returns whether or not the description should be hidden from the edit page.
		/// </summary>
		public bool HideDescription => IsQuestionTemplate || HasChoices;

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public abstract string QuestionTypeDisplay { get; }

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public abstract string QuestionTypeDescription { get; }

		/// <summary>
		/// Accepts a question visitor.
		/// </summary>
		public abstract void Accept(IQuestionVisitor questionVisitor);

		/// <summary>
		/// Accepts a question visitor that returns a result.
		/// </summary>
		public abstract TResult Accept<TResult>(IQuestionResultVisitor<TResult> questionVisitor);

		/// <summary>
		/// Accepts a question visitor that returns a result.
		/// </summary>
		public abstract TResult Accept<TResult, TParam1>(
			IQuestionResultVisitor<TResult, TParam1> questionVisitor,
			TParam1 param1);
	}
}
