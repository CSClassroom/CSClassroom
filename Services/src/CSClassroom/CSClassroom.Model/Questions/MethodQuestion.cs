using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// An exercise that requires a method to be written.
	/// </summary>
	public class MethodQuestion : CodeQuestion
	{
		/// <summary>
		/// The name of the method.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Method Name",
			Description = "Enter the name of method to be written."
		)]
		public string MethodName { get; set; }

		/// <summary>
		/// A comma-seaparated list of parameters of the method.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Parameter Types",
			Description = "Enter the types of parameters for the method, separated by commas."
		)]
		public string ParameterTypes { get; set; }

		/// <summary>
		/// The return type of the method.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Return Type",
			Description = "Enter the return type of the method."
		)]
		public string ReturnType { get; set; }

		/// <summary>
		/// Test for this question.
		/// </summary>
		[Display
		(
			Name = "Tests",
			Description = "Enter the tests for this method. Parameter values should be separated by commas."
		)]
		public virtual IList<MethodQuestionTest> Tests { get; set; }

		/// <summary>
		/// Returns whether or not this question is a template for another question.
		/// </summary>
		public override bool IsQuestionTemplate => false;

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public override string QuestionTypeDisplay => "Method Question";

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public override string QuestionTypeDescription =>
			"A question that requires a method to be written. "
			+ "The signature of the method will be validated, and a "
			+ "series of test cases will be run to ensure correctness.";

		/// <summary>
		/// The HTML string displayed when solving each type of question.
		/// </summary>
		public override string SubmissionTypeDescription =>
			"This question asks you to write a <strong>single method</strong>. "
			+ "Do <strong>not</strong> include a full program, a class, or a main method.";

		/// <summary>
		/// Returns a list of tests for this code question.
		/// </summary>
		public override IEnumerable<CodeQuestionTest> GetTests()
		{
			return Tests;
		}

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
