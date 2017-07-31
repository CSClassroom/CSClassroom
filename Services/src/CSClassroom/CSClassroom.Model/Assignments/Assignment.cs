using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Assignments
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
		[MaxLength(100)]
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
		[MaxLength(100)]
		[Display
		(
			Name = "Group Name",
			Description = "Enter the group name for the assignment. Assignments with the same "
				+ "group name will be grouped together for scoring purposes. If left blank, "
				+ "the name of the assignment will be used."
		)]
		public string GroupName { get; set; }

		/// <summary>
		/// Whether or not the assignment is visible to students.
		/// </summary>
		[Display
		(
			Name = "Private",
			Description = "Select whether or not the assignment should be visible to students."
		)]
		public bool IsPrivate { get; set; }

		/// <summary>
		/// The maximum number of attempts per question.
		/// </summary>
		[Display
		(
			Name = "Attempts Per Question",
			Description = "The maximum number of attempts permitted for each question (0 for unlimited). "
		)]
		public int? MaxAttempts { get; set; }

		/// <summary>
		/// Whether or not all answers should be submitted on the same page.
		/// </summary>
		[Display
		(
			Name = "Combine Submissions",
			Description = "Select whether or not all questions should appear on the same page. "
				+ "Assignments that use this option cannot have code questions. "
		)]
		public bool CombinedSubmissions { get; set; }

		/// <summary>
		/// Whether or not to show past submissions for non-code questions.
		/// </summary>
		[Display
		(
			Name = "Only Show Combined Score",
			Description = "Select whether or not to only show the total score when submissions are combined. "
				+ "If true, answers and per-question scores are not shown. This option is only available "
				+ "when submissions are combined."
		)]
		public bool OnlyShowCombinedScore { get; set; }

		/// <summary>
		/// Whether or not all answers should be submitted on the same page.
		/// </summary>
		[Display
		(
			Name = "Answer in Order",
			Description = "Select whether or not students must answer the questions "
				+ "on this assignment in order. This option is not available when submissions "
				+ "are combined."
		)]
		public bool AnswerInOrder { get; set; }

		/// <summary>
		/// The questions for this assignment.
		/// </summary>
		[Display
		(
			Name = "Questions",
			Description = "Enter the questions that will be part of this assignment."
		)]
		public virtual IList<AssignmentQuestion> Questions { get; set; }

		/// <summary>
		/// The due dates for each section.
		/// </summary>
		[Display
		(
			Name = "Due Dates",
			Description = "Enter a due date for each section assigned this assignment."
		)]
		public virtual ICollection<AssignmentDueDate> DueDates { get; set; }

		/// <summary>
		/// Returns the due date of the assignment in the given section, or null if
		/// the assignment is not assigned for the given section. If section is not
		/// specified, the latest due date of all sections is returned (or null if
		/// the assignment has no due date for any section).
		/// </summary>
		public DateTime? GetDueDate(Section section)
		{
			if (section != null)
			{
				return DueDates
					?.SingleOrDefault(d => d.SectionId == section.Id)
					?.DueDate;
			}
			else if ((DueDates?.Count ?? 0) > 0)
			{
				return DueDates.Max(d => d.DueDate);
			}
			else
			{
				return null;
			}
		}
	}
}

