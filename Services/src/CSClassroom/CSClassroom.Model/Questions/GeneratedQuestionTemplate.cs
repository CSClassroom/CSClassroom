using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A question that is randomly generated, based on a template.
	/// </summary>
	public class GeneratedQuestionTemplate : CodeQuestion
	{
		/// <summary>
		/// A java class called QuestionGenerator, which must contain a static generateQuestion method.
		/// This method accepts an integer seed, and returns a question.
		/// </summary>
		[Display
		(
			Name = "Question Generator",
			Description = "Write a class called QuestionGenerator, containing a single public static method "
				+ "called generateQuestion. The method should take an integer, representing a random integer. "
				+ "The method should return a Question object. For more information, see the documentation "
				+ "for the question model below."
		)]
		public string GeneratorContents { get; set; }

		/// <summary>
		/// The full generator file used to generate the question,
		/// including the question model classes used by the 
		/// generator contents. This is used to generate the question,
		/// and saved separately to allow for future modifications
		/// to the question model without invalidating all generated questions.
		/// </summary>
		public string FullGeneratorFileContents { get; set; }

		/// <summary>
		/// The line offset of the full generator file.
		/// </summary>
		public int FullGeneratorFileLineOffset { get; set; }

		/// <summary>
		/// The date the question was modified.
		/// </summary>
		public DateTime DateModified { get; set; }

		/// <summary>
		/// Returns whether or not this question is a template for another question.
		/// </summary>
		public override bool IsQuestionTemplate => true;

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public override string QuestionTypeDisplay => "Generated Question";

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public override string QuestionTypeDescription =>
			"A question that is programatically generated, using custom-written java code."
			+ "The generated question can be any of the other questions types.";

		/// <summary>
		/// The HTML string displayed when solving each type of question.
		/// </summary>
		public override string SubmissionTypeDescription => string.Empty;

		/// <summary>
		/// Returns a list of tests for this code question.
		/// </summary>
		public override IEnumerable<CodeQuestionTest> GetTests() => null;

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
