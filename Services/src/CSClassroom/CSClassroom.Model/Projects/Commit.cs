using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A commit of a project.
	/// </summary>
	public class Commit
	{
		/// <summary>
		/// The ID of the build.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The project for which this build belongs to.
		/// </summary>
		public int ProjectId { get; set; }

		/// <summary>
		/// The project for which this build belongs to.
		/// </summary>
		public virtual Project Project { get; set; }

		/// <summary>
		/// The user for which this build belongs to.
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// The user whose commit prompted the build.
		/// </summary>
		public virtual User User { get; set; }

		/// <summary>
		/// The date of the push operation that included this commit.
		/// </summary>
		public DateTime PushDate { get; set; }

		/// <summary>
		/// The date of the commit.
		/// </summary>
		public DateTime CommitDate { get; set; }

		/// <summary>
		/// The commit message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The SHA of the commit.
		/// </summary>
		public string Sha { get; set; }

		/// <summary>
		/// The build request token. This is required for a build worker
		/// to report a build result.
		/// </summary>
		public string BuildRequestToken { get; set; }

		/// <summary>
		/// The ID of the build job.
		/// </summary>
		public string BuildJobId { get; set; }

		/// <summary>
		/// The build corresponding to this commit.
		/// </summary>
		public virtual Build Build { get; set; }

		/// <summary>
		/// The submissions of this commit.
		/// </summary>
		public virtual IList<Submission> Submissions { get; set; }

		/// <summary>
		/// Returns the repository name for a given commit.
		/// </summary>
		public string GetRepoName()
		{
			var projectName = Project.Name;

			var teamName = User
				.ClassroomMemberships
				.Single(cm => cm.ClassroomId == Project.ClassroomId)
				.GitHubTeam;

			return $"{projectName}_{teamName}";
		}
	}
}
