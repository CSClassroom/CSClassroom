using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Classrooms
{
	/// <summary>
	/// A section of students for a classroom.
	/// </summary>
	public class Section
	{
		/// <summary>
		/// The unique ID for the section.
		/// </summary>
		[Required]
		public int Id { get; set; }

		/// <summary>
		/// The ID of the classroom containing this section.
		/// </summary>
		[Required]
		public int ClassroomId { get; set; }

		/// <summary>
		/// The classroom that contains this section.
		/// </summary>
		public virtual Classroom Classroom { get; set; }

		/// <summary>
		/// The name of the section.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = ModelStrings.OnlyAlphanumeric)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the section that will appear in URLs. This must be unique for the class."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The display name of the section.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Display Name",
			Description = "Enter the name of the section to be displayed on the website."
		)]
		public string DisplayName { get; set; }

		/// <summary>
		/// Whether or not to allow registrations for this section.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Allow New Registrations",
			Description = "Select whether or not to allow new registrations for this section."
		)]
		public bool AllowNewRegistrations { get; set; }
		
		/// <summary>
		/// Whether or not to allow students to send messages to admins.
		/// </summary>
		[Display
		(
			Name = "Allow Student Messages", 
			Description = "Select whether or not to allow students to send messages to admins."
		)]
		public bool AllowStudentMessages { get; set; }

		/// <summary>
		/// The section memberships for this section.
		/// </summary>
		public IList<SectionMembership> SectionMemberships { get; set; }

		/// <summary>
		/// The section recipients for this section.
		/// </summary>
		[Display
		(
			Name = "Section Recipients",
			Description = "Select the class admins who will receive announcements and student "
				+ "messages for this section."
		)]
		public IList<SectionRecipient> SectionRecipients { get; set; }

		/// <summary>
		/// The gradebooks for this section.
		/// </summary>
		[Display
		(
			Name = "Section Gradebooks",
			Description = "Enter the last transfer date of assignment grades to each gradebook."
		)]
		public IList<SectionGradebook> SectionGradebooks { get; set; }
	}
}
