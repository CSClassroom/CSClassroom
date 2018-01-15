using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// Represents a class admin that has chosen to receive messages
	/// from students in a particular section.
	/// </summary>
	public class SectionRecipient
	{
		/// <summary>
		/// The ID of the section message recipient.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The recipient.
		/// </summary>
		[Display(Name = "Admin")]
		public int ClassroomMembershipId { get; set; }
		public ClassroomMembership ClassroomMembership { get; set; }

		/// <summary>
		/// The section for which the user will receive messages.
		/// </summary>
		public int SectionId { get; set; }
		public Section Section { get; set; }
		
		/// <summary>
		/// Whether or not to view announcements to this section.
		/// </summary>
		[Display(Name = "View Announcements")]
		public bool ViewAnnouncements { get; set; }
		
		/// <summary>
		/// Whether or not to receive e-mails for announcements to this section.
		/// </summary>
		[Display(Name = "E-mail Announcements")]
		public bool EmailAnnouncements { get; set; }
		
		/// <summary>
		/// Whether or not to view messages from students in this section.
		/// </summary>
		[Display(Name = "View Messages")]
		public bool ViewMessages { get; set; }
		
		/// <summary>
		/// Whether or not to receive e-mails for messages from students in this section.
		/// </summary>
		[Display(Name = "E-mail Messages")]
		public bool EmailMessages { get; set; }
	}
}
