using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Communications
{
	/// <summary>
	/// An announcement to a class.
	/// </summary>
	public class Announcement
	{
		/// <summary>
		/// The ID of the announcement.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The announcement's title.
		/// </summary>
		[Required]
		[MaxLength(100)]
		[Display
		(
			Name = "Title",
			Description = "Enter the title of the announcement."
		)]
		public string Title { get; set; }

		/// <summary>
		/// The HTML contents of the announcement.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Contents",
			Description = "Enter the contents of the announcement."
		)]
		public string Contents { get; set; }

		/// <summary>
		/// The classroom the announcement appears in.
		/// </summary>
		public int ClassroomId { get; set; }

		/// <summary>
		/// The date that the announcement was posted.
		/// </summary>
		public DateTime DatePosted { get; set; }

		/// <summary>
		/// The classroom the announcement appears in.
		/// </summary>
		public Classroom Classroom { get; set; }

		/// <summary>
		/// The user who wrote the announcement.
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// THe user who wrote the announcement.
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// The sections that the announcement appears in.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Sections",
			Description = "Select the sections that will show the announcement."
		)]
		public IList<AnnouncementSection> Sections { get; set; }
	}
}
