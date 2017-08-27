using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Communications
{
	/// <summary>
	/// A relationship between an announcement and a section.
	/// </summary>
	public class AnnouncementSection
	{
		/// <summary>
		/// The ID of the announcement/section relationship.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the announcement.
		/// </summary>
		public int AnnouncementId { get; set; }

		/// <summary>
		/// The announcement.
		/// </summary>
		public Announcement Announcement { get; set; }

		/// <summary>
		/// The ID of the section the announcement appears in.
		/// </summary>
		public int SectionId { get; set; }

		/// <summary>
		/// The section the announcement appears in.
		/// </summary>
		public Section Section { get; set; }
	}
}
