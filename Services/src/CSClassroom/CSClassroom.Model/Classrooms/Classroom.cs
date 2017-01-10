using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Model.Classrooms
{
	/// <summary>
	/// A single classroom (potentially with multiple sections of students).
	/// </summary>
	public class Classroom
	{
		/// <summary>
		/// The unique ID for the classroom.
		/// </summary>
		[Required]
		public int Id { get; set; }

		/// <summary>
		/// The name of the classroom.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = ModelStrings.OnlyAlphanumeric)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the class that will appear in URLs. This must be unique."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The display name of the classroom.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Display Name",
			Description = "Enter the name of the class to be displayed on the website."
		)]
		public string DisplayName { get; set; }

		/// <summary>
		/// The GitHub organization associated with the classroom.
		/// </summary>
		[Required]
		[Display
		(
			Name = "GitHub Organization",
			Description = "Enter the name of the GitHub organization associated with this classroom."
		)]
		public string GitHubOrganization { get; set; }

		/// <summary>
		/// The default time for due dates.
		/// </summary>
		[Display
		(
			Name = "Default Time Due",
			Description = "Select the default time of day that assignments and projects will be due."
		)]
		public TimeSpan DefaultTimeDue { get; set; }

		/// <summary>
		/// The sections in this classroom.
		/// </summary>
		public virtual ICollection<Section> Sections { get; set; }

		/// <summary>
		/// The categories of questions available in this classroom.
		/// </summary>
		public virtual ICollection<QuestionCategory> Categories { get; set; }

		/// <summary>
		/// The gradebooks for this classroom.
		/// </summary>
		[Display
		(
			Name = "Classroom Gradebooks",
			Description = "Enter the name of one or more external gradebooks. "
				+ "This will facilitate transferring assignment grades to the gradebook."
		)]
		public virtual IList<ClassroomGradebook> ClassroomGradebooks { get; set; }
	}
}
