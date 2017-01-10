using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.Service.Projects.PushEvents
{
	/// <summary>
	/// A commit that we were notified of through a push event.
	/// </summary>
	public class PushEventCommit
	{
		/// <summary>
		/// The push event that notified us of the commit.
		/// </summary>
		public GitHubPushEvent PushEvent { get; }

		/// <summary>
		/// The commit that needs to be processed.
		/// </summary>
		public Commit Commit { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PushEventCommit(GitHubPushEvent pushEvent, Commit commit)
		{
			PushEvent = pushEvent;
			Commit = commit;
		}
	}
}
