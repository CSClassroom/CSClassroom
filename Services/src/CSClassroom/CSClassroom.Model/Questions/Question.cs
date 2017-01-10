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
		[Required]
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
			Name = "Name",
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
		/// Whether or not the category is public.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Private",
			Description = "Select whether or not the question should be hidden from view."
		)]
		public bool IsPrivate { get; set; }
		
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
		/// All questions that must be answered before this question is answered.
		/// </summary>
		[Display
		(
			Name = "Prerequisites",
			Description = "Select which questions (if any) must be answered correctly before this question can be seen."
		)]
		public IList<PrerequisiteQuestion> PrerequisiteQuestions { get; set; }

		/// <summary>
		/// All questions that may be answered only after this question is answered.
		/// </summary>
		public IList<PrerequisiteQuestion> SubsequentQuestions { get; set; }

		/// <summary>
		/// Returns whether or not this question is a template for another question.
		/// </summary>
		public abstract bool IsQuestionTemplate { get; }

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
