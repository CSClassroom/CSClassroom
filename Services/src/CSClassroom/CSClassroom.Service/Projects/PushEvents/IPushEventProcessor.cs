using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Service.Projects.Repositories;

namespace CSC.CSClassroom.Service.Projects.PushEvents
{
	/// <summary>
	/// Creates build jobs for a set of push events.
	/// </summary>
	public interface IPushEventProcessor
	{
		/// <summary>
		/// Given a list of push events, along with a list of 
		/// existing recorded commits, returns a list of commits
		/// that need to be processed.
		/// <param name="project">The project whose commits we are processing.</param>
		/// <param name="repoEventLists">The push events to process if needed.</param>
		/// <param name="existingCommits">The commits that have already been processed.</param>
		/// </summary>
		IList<PushEventCommit> GetNewCommitsToProcess(
			Project project,
			ICollection<CommitDescriptor> existingCommits,
			IList<StudentRepoPushEvents> repoEventLists);

		/// <summary>
		/// Creates a build job for a new commit received by a push event.
		/// Returns the job ID for the build job.
		/// </summary>
		Task<string> CreateBuildJobAsync(
			Project project,
			PushEventCommit newCommit,
			string buildResultCallbackUrl);
	}
}