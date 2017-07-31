using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Assignments
{
	/// <summary>
	/// The class submission type
	/// </summary>
	public enum ClassSubmissionType
	{
		[Display(Name = "Custom")]
		Custom,

		[Display(Name = "Full Class")]
		FullClass,

		[Display(Name = "Partial Class")]
		PartialClass,

		[Display(Name = "Code Fragment")]
		CodeFragment,

		[Display(Name = "Full Program")]
		FullProgram,

		[Display(Name = "Single Method")]
		SingleMethod,
	}
}
