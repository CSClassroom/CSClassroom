using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// An authorization for a user in a particular classroom.
	/// </summary>
	public class ClassroomMembership
	{
		/// <summary>
		/// The unique ID for the classroom authorization.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The user to authorize.
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// The user to authorize.
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// The classroom.
		/// </summary>
		public int ClassroomId { get; set; }

		/// <summary>
		/// The classroom.
		/// </summary>
		public virtual Classroom Classroom { get; set; }

		/// <summary>
		/// The role of the user for this membership.
		/// </summary>
		public ClassroomRole Role { get; set; }

		/// <summary>
		/// Whether or not the user is in the GitHub organization.
		/// </summary>
		public bool InGitHubOrganization { get; set; }

		/// <summary>
		/// The suffix to append to each of the user's repository names.
		/// </summary>
		public string GitHubTeam { get; set; }

		/// <summary>
		/// A list of section memberships for this user.
		/// </summary>
		public virtual List<SectionMembership> SectionMemberships { get; set; }
	}
}
