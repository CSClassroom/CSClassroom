using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Repositories;

namespace CSC.CSClassroom.Service.Projects.PushEvents
{
	/// <summary>
	/// GitHub operations related to pushes of commits.
	/// </summary>
	public class PushEventRetriever : IPushEventRetriever
	{
		/// <summary>
		/// The repository metadata retriever.
		/// </summary>
		private readonly IRepositoryMetadataRetriever _repoMetadataRetriever;

		/// <summary>
		/// The repository client.
		/// </summary>
		private readonly IGitHubRepositoryClient _repoClient;

		/// <summary>
		/// The operation runner.
		/// </summary>
		private readonly IOperationRunner _operationRunner;

		/// <summary>
		/// Constructor.
		/// </summary>
		public PushEventRetriever(
			IRepositoryMetadataRetriever repoMetadataRetriever, 
			IGitHubRepositoryClient repoClient,
			IOperationRunner operationRunner)
		{
			_repoMetadataRetriever = repoMetadataRetriever;
			_repoClient = repoClient;
			_operationRunner = operationRunner;
		}

		/// <summary>
		/// Returns a list of push events for the given project.
		/// </summary>
		public async Task<IList<StudentRepoPushEvents>> GetAllPushEventsAsync(
			Project project,
			IList<ClassroomMembership> students)
		{
			var studentRepos = await _repoMetadataRetriever.GetStudentRepositoriesAsync
			(
				project, 
				students
			);

			return await _operationRunner.RunOperationsAsync
			(
				studentRepos.Keys,
				student => GetAllPushEventsAsync
				(
					student,
					studentRepos[student]
				)
			);
		}

		/// <summary>
		/// Returns a list of push events for the given project.
		/// </summary>
		private async Task<StudentRepoPushEvents> GetAllPushEventsAsync(
			ClassroomMembership student,
			GitHubRepository repository)
		{
			var pushEvents = await _repoClient.GetPushEventsAsync
			(
				repository.Owner, 
				repository.Name
			);

			return new StudentRepoPushEvents(student, pushEvents);
		}
	}
}
