using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// An assignment of exercises.
	/// </summary>
	public class Assignment
	{
		/// <summary>
		/// The ID of the assignment.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The classroom for the assignment.
		/// </summary>
		public int ClassroomId { get; set; }

		/// <summary>
		/// The classroom for the assignment.
		/// </summary>
		public virtual Classroom Classroom { get; set; }

		/// <summary>
		/// The name of the assignment.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the assignment."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The group name for the assignment. Assignments in the same group will 
		/// be given a combined score.
		/// </summary>
		[MaxLength(50)]
		[Display
		(
			Name = "Group Name",
			Description = "Enter the group name for the assignment. Assignments with the same "
				+ "group name will be grouped together for scoring purposes. If left blank, "
				+ "the name of the assignment will be used."
		)]
		public string GroupName { get; set; }

		/// <summary>
		/// The questions for this assignment.
		/// </summary>
		[Display
		(
			Name = "Questions",
			Description = "Enter the questions that will be part of this assignment."
		)]
		public virtual ICollection<AssignmentQuestion> Questions { get; set; }

		/// <summary>
		/// The due dates for each section.
		/// </summary>
		[Display
		(
			Name = "Due Dates",
			Description = "Enter a due date for each section assigned this assignment."
		)]
		public virtual ICollection<AssignmentDueDate> DueDates { get; set; }
	}
}
