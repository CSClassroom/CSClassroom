using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Assignments
{
	/// <summary>
	/// A constraint on the code that may be submitted
	/// for a code question.
	/// </summary>
	public class CodeConstraint
	{
		/// <summary>
		/// The unique identifier for a code constraint. 
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the code question the constraint applies to.
		/// </summary>
		public int CodeQuestionId { get; set; }

		/// <summary>
		/// The code question containing this code constraint.
		/// </summary>
		public CodeQuestion CodeQuestion { get; set; }

		/// <summary>
		/// The regular expression to match.
		/// </summary>
		[Required]
		[Display(Name = "Regular Expression")]
		public string Regex { get; set; }

		/// <summary>
		/// The type of code constraint.
		/// </summary>
		[Required]
		[Display(Name = "Type")]
		public CodeConstraintType Type { get; set; }

		/// <summary>
		/// The frequency permitted (or required).
		/// </summary>
		[Required]
		[Display(Name = "Frequency")]
		public int Frequency { get; set; }

		/// <summary>
		/// The error message if the constraint is violated.
		/// </summary>
		[Required]
		[Display(Name = "Error Message")]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// The order the constraint is evaluated in.
		/// </summary>
		[Required]
		public int Order { get; set; }
	}
}
