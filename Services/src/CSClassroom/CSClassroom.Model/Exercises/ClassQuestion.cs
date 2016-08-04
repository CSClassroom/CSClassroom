using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Exercises
{
	/// <summary>
	/// An exercise that requires a class (or part of a class)
	/// to be written.
	/// </summary>
	public class ClassQuestion : CodeQuestion
	{
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
			+ "The string %SUBMISSION% must be included exactly once, and will be replaced with the submitted class."
		)]
		public string FileTemplate { get; set; }

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
	}
}
