using System;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Allows the user to select an assignment report.
	/// </summary>
	public class SelectAssignmentReport
	{
		/// <summary>
		/// The section name.
		/// </summary>
		[Display
		(
			Name = "Section",
			Description = "Select a section."
		)]
		public string SectionName { get; set; }

		/// <summary>
		/// The assignment group name.
		/// </summary>
		[Display
		(
			Name = "Assignment",
			Description = "Select an assignment."
		)]
		public string AssignmentGroupName { get; set; }

		/// <summary>
		/// The gradebook.
		/// </summary>
		[Display
		(
			Name = "Gradebook",
			Description = "Select a gradebook."
		)]
		public string GradebookName { get; set; }

		/// <summary>
		/// The date to mark assignments as last transferred.
		/// </summary>
		[Display(Name = "Update Last Transfer Date")]
		public DateTime? LastTransferDate { get; set; }
	}
}
