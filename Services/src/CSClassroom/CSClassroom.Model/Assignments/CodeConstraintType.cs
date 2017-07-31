using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// The type of code constraint.
	/// </summary>
	public enum CodeConstraintType
	{
		/// <summary>
		/// A constraint that requires the substring to appear
		/// at least a given number of times
		/// </summary>
		[Display(Name = "At Least")]
		AtLeast = 0,

		/// <summary>
		/// A constraint that requires the substring to appear
		/// exactly a given number of times
		/// </summary>
		[Display(Name = "Exactly")]
		Exactly = 1,

		/// <summary>
		/// A constraint that requires the substring to appear
		/// at most a given number of times
		/// </summary>
		[Display(Name = "At Most")]
		AtMost = 2
	}
}
