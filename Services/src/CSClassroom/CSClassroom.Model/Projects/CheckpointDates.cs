using System;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// The start date and due date for a given section/checkpoint.
	/// </summary>
	public class CheckpointDates
	{
		/// <summary>
		/// The unique ID for the checkpoint.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The checkpoint ID.
		/// </summary>
		public int CheckpointId { get; set; }

		/// <summary>
		/// The checkpoint.
		/// </summary>
		public virtual Checkpoint Checkpoint { get; set; }

		/// <summary>
		/// The section for this checkpoint.
		/// </summary>
		[Display(Name = "Section")]
		public int SectionId { get; set; }

		/// <summary>
		/// The section for this checkpoint.
		/// </summary>
		public virtual Section Section { get; set; }

		/// <summary>
		/// The start date for the checkpoint (or null, if the checkpoint 
		/// includes all commits from the beginning).
		/// </summary>
		[Display(Name = "Start Date (blank for beginning)")]
		public DateTime? StartDate { get; set; }

		/// <summary>
		/// The due date for the checkpoint.
		/// </summary>
		[Display(Name = "Due Date")]
		public DateTime DueDate { get; set; }
	}
}
