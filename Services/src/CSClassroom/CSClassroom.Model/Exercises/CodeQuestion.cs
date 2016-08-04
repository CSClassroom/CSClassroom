using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Exercises
{
	/// <summary>
	/// An exercise that involves writing code.
	/// </summary>
	/// <typeparam name="TTestType">The type of tests that the exercise has.</typeparam>
	public abstract class CodeQuestion : Question
	{
		/// <summary>
		/// A list of classes to import (possibly including wildcards),
		/// separated by whitespace.
		/// </summary>
		[Display
		(
			Name = "Imported Classes",
			Description = "Enter each class to import on a separate line (if any). Wildcards, such as java.util.*, are permitted."
		)]
		public string ImportedClasses { get; set; }

		/// <summary>
		/// Test for this question.
		/// </summary>
		public ICollection<CodeQuestion> Tests { get; set; }
	}
}
