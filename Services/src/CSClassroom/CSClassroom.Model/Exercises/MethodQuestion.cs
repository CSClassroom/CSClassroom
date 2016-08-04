using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Exercises
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
	}
}
