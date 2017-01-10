using System;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// The status of a gradebook for a section.
	/// </summary>
	public class SectionGradebook
	{
		/// <summary>
		/// The unique ID for the gradebook.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The classroom gradebook ID.
		/// </summary>
		[Display(Name = "Gradebook")]
		public int ClassroomGradebookId { get; set; }

		/// <summary>
		/// The classroom gradebook.
		/// </summary>
		public virtual ClassroomGradebook ClassroomGradebook { get; set; }

		/// <summary>
		/// The section ID.
		/// </summary>
		public int SectionId { get; set; }

		/// <summary>
		/// The section.
		/// </summary>
		public virtual Section Section { get; set; }

		/// <summary>
		/// The date assignments were last transferred to the gradebook.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Last Transfer Date"
		)]
		public DateTime LastTransferDate { get; set; }
	}
}
