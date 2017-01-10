using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A project checkpoint.
	/// </summary>
	public class Checkpoint
	{
		/// <summary>
		/// The primary key.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the project that this checkpoint belongs to.
		/// </summary>
		public int ProjectId { get; set; }

		/// <summary>
		/// The project for this checkpoint.
		/// </summary>
		public virtual Project Project { get; set; }

		/// <summary>
		/// The name of the checkpoint.
		/// </summary>
		[MaxLength(50)]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = ModelStrings.OnlyAlphanumeric)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the checkpoint that will appear in URLs "
				+ "and GitHub branch names. This must be unique for the class."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The display name of the checkpoint.
		/// </summary>
		[Display
		(
			Name = "Display Name",
			Description = "Enter the name of the checkpoint to be displayed on the website."
		)]
		public string DisplayName { get; set; }

		/// <summary>
		/// Dates for each section that has the checkpoint assigned.
		/// </summary>
		[Display
		(
			Name = "Section Dates",
			Description = "Enter the checkpoint dates for each section."
		)]
		public virtual IList<CheckpointDates> SectionDates { get; set; }

		/// <summary>
		/// Dates for each section that has the checkpoint assigned.
		/// </summary>
		[Display
		(
			Name = "Test Classes",
			Description = "Enter the applicable test classes for this checkpoint."
		)]
		public virtual IList<CheckpointTestClass> TestClasses { get; set; }

		/// <summary>
		/// Submissions for this checkpoint.
		/// </summary>
		public virtual IList<Submission> Submissions { get; set; }
	}
}