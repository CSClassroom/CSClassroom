using System;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A due date for an assignment, for a given section of a class.
	/// </summary>
	public class AssignmentDueDate
	{
		/// <summary>
		/// The unique ID for the section assignment.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The assignment ID.
		/// </summary>
		public int AssignmentId { get; set; }

		/// <summary>
		/// The assignment.
		/// </summary>
		public virtual Assignment Assignment { get; set; }

		/// <summary>
		/// The section for this assignment.
		/// </summary>
		[Display(Name = "Section")]
		public int SectionId { get; set; }

		/// <summary>
		/// The section for this assignment.
		/// </summary>
		public virtual Section Section { get; set; }

		/// <summary>
		/// The due date for the assignment.
		/// </summary>
		[Display(Name = "Due Date")]
		public virtual DateTime DueDate { get; set; }
	}
}
