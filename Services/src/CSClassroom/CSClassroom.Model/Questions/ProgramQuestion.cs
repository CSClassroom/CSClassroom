using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// An exercise that requires an entire program (with main method)
	/// to be written.
	/// </summary>
	public class ProgramQuestion : CodeQuestion
	{
		/// <summary>
		/// The name of the class containing the main method.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Class Name",
			Description = "Enter the name of the class that will contain the main method."
		)]
		public string ProgramClassName { get; set; }

		/// <summary>
		/// Tests for this question.
		/// </summary>
		[Display
		(
			Name = "Tests",
			Description = "Enter the tests for this class."
		)]
		public virtual IList<ProgramQuestionTest> Tests { get; set; }

		/// <summary>
		/// Returns whether or not this question is a template for another question.
		/// </summary>
		public override bool IsQuestionTemplate => false;

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public override string QuestionTypeDisplay => "Program Question";

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public override string QuestionTypeDescription =>
			"A question that requires an entire program (including a main method) to be written. "
			+ "The output of the main method will be compared against the expected output.";

		/// <summary>
		/// The HTML string displayed when solving each type of question.
		/// </summary>
		public override string SubmissionTypeDescription =>
			"This questions asks you to write an <strong>entire program</strong>. "
			+ "You <strong>must</strong> include a class with a main method.";

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
