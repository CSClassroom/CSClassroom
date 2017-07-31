using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Assignments
{
	/// <summary>
	/// An exercise that involves writing code.
	/// </summary>
	public abstract class CodeQuestion : Question
	{
		/// <summary>
		/// A list of classes to import (possibly including wildcards),
		/// separated by whitespace.
		/// </summary>
		[Display
		(
			Name = "Imported Classes",
			Description = "Enter each class to import, if any. Wildcards, "
				+"such as java.util.*, are permitted."
		)]
		public virtual List<ImportedClass> ImportedClasses { get; set; }

		/// <summary>
		/// The initial submission to be populated when students first 
		/// attempt the question.
		/// </summary>
		[Display
		(
			Name = "Default Submission",
			Description = "Enter the initial submission to be populated when students first attempt "
				+ "to answer the question. This may be left blank."
		)]
		public string InitialSubmission { get; set; }

		/// <summary>
		/// Constraints on code submitted for this question.
		/// </summary>
		[Display
		(
			Name = "Code Constraints",
			Description = "Enter any constraints required to be met on code "
				+ "submitted to answer this question."
		)]
		public virtual List<CodeConstraint> CodeConstraints { get; set; }

		/// <summary>
		/// Returns a list of tests for this code question.
		/// </summary>
		public abstract IEnumerable<CodeQuestionTest> GetTests();

		/// <summary>
		/// The HTML string displayed when solving each type of question.
		/// </summary>
		public abstract string SubmissionTypeDescription { get; }

		/// <summary>
		/// Returns whether or not the question is a randomly selected question.
		/// </summary>
		public override bool HasChoices => false;

		/// <summary>
		/// Returns whether or not this question can be duplicated.
		/// </summary>
		public override bool CanDuplicate => true;

		/// <summary>
		/// Returns whether or not this question can be turned into a generated question template.
		/// </summary>
		public override bool CanRandomize => false;
	}
}
