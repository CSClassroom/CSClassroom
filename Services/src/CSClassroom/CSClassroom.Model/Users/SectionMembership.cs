using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// An authorization for a user in a particular section.
	/// </summary>
	public class SectionMembership
	{
		/// <summary>
		/// The unique ID for the section authorization.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The classroom membership ID for this section membership.
		/// </summary>
		public int ClassroomMembershipId { get; set; }

		/// <summary>
		/// The classroom membership for this section membership.
		/// </summary>
		public virtual ClassroomMembership ClassroomMembership { get; set; }

		/// <summary>
		/// The section to provide access to.
		/// </summary>
		public int SectionId { get; set; }

		/// <summary>
		/// The section to provide access to.
		/// </summary>
		public virtual Section Section { get; set; }

		/// <summary>
		/// The role of the user for this membership.
		/// </summary>
		public SectionRole Role { get; set; }
	}
}