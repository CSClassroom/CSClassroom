using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// An exercise that requires a class (or part of a class)
	/// to be written.
	/// </summary>
	public class ClassQuestion : CodeQuestion
	{
		/// <summary>
		/// The class question type
		/// </summary>
		[Required]
		[Display
		(
			Name = "Question Type",
			Description = "Select the type of question."
		)]
		public ClassSubmissionType ClassSubmissionType { get; set; }

		/// <summary>
		/// The name of the class to be written.
		/// </summary>
		[Required]
		[Display
		(
			Name="Class Name",
			Description = "Enter the name of the class to be written."
		)]
		public string ClassName { get; set; }

		/// <summary>
		/// A template for the java file to compile, without imports.
		/// This template must contain the string %SUBMISSION%, as that
		/// will be the string that is replaced with the submission for
		/// the exercise. This must contain a single public class, whose
		/// name must be this exercise's class name. It can also contain
		/// any number of non-public classes.
		/// </summary>
		[Required]
		[Display
		(
			Name = "File Template",
			Description = "Enter the template java file that surrounds the class to be written. "
			+ "This template should not include import statements."
			+ "The string " + SubmissionPlaceholder + " must be included exactly once, and will be replaced with the submitted class."
		)]
		public string FileTemplate { get; set; }

		[Required]
		[Display
		(
			Name = "Allow Public Fields",
			Description = "Select whether or not to allow public fields in the submitted class."
		)]
		public bool AllowPublicFields { get; set; }

		/// <summary>
		/// The required methods for this class.
		/// </summary>
		[Display
		(
			Name = "Required Methods",
			Description = "Enter the methods required to be implemented. Parameter types should be separated by commas."
		)]
		public virtual IList<RequiredMethod> RequiredMethods { get; set; }

		/// <summary>
		/// Test for this question.
		/// </summary>
		[Display
		(
			Name = "Tests",
			Description = "Enter the tests for this class."
		)]
		public virtual IList<ClassQuestionTest> Tests { get; set; }

		/// <summary>
		/// Returns whether or not this question is a template for another question.
		/// </summary>
		public override bool IsQuestionTemplate => false;

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public override string QuestionTypeDisplay => "Class Question";

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public override string QuestionTypeDescription =>
			"A question that requires a class to be written. "
			+ "The fields and methods of the class will be validated, "
			+ "and a series of test cases will be run to ensure correctness.";


		/// <summary>
		/// The HTML string displayed when solving each type of question.
		/// </summary>
		public override string SubmissionTypeDescription
		{
			get
			{
				switch (ClassSubmissionType)
				{
					case ClassSubmissionType.FullClass:
						return "This question asks you to write a <strong>full class</strong>. "
						+ "Be sure to include any methods specified in the problem statement.";

					case ClassSubmissionType.PartialClass:
						return "This question asks you to write a <strong>partial class</strong>. "
						+ "Only include the methods described in the problem statement.";

					case ClassSubmissionType.CodeFragment:
						return "This question asks you to write a <strong>code fragment</strong>. "
						+ "Do <strong>not</strong> include any method or class in your submission. "
						+ "Instead, just include a fragment of code that solves the problem.";

					case ClassSubmissionType.FullProgram:
						return new ProgramQuestion().SubmissionTypeDescription;

					case ClassSubmissionType.SingleMethod:
						return new MethodQuestion().SubmissionTypeDescription;

					default:
						return string.Empty;
				}
			}
		}

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

		/// <summary>
		/// The placeholder string for a submission.
		/// </summary>
		public const string SubmissionPlaceholder = "%SUBMISSION%";
	}
}
