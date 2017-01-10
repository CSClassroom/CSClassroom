using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The status of a project's build.
	/// </summary>
	public class ProjectStatusResults
	{
		/// <summary>
		/// The user's last name.
		/// </summary>
		public string LastName { get; }

		/// <summary>
		/// The user's first name.
		/// </summary>
		public string FirstName { get; }

		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// The status of each project.
		/// </summary>
		public IList<ProjectStatus> ProjectStatus { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectStatusResults(
			string lastName,
			string firstName,
			int userId,
			IList<ProjectStatus> projectStatus)
		{
			LastName = lastName;
			FirstName = firstName;
			UserId = userId;
			ProjectStatus = projectStatus;
		}
	}
}
