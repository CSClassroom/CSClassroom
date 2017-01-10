using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Repositories;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// Downloads submissions for students.
	/// </summary>
	public class SubmissionDownloader : ISubmissionDownloader
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
		public SubmissionDownloader(
			IRepositoryMetadataRetriever repoMetadataRetriever,
			IGitHubRepositoryClient repoClient, 
			IOperationRunner operationRunner)
		{
			_repoMetadataRetriever = repoMetadataRetriever;
			_repoClient = repoClient;
			_operationRunner = operationRunner;
		}

		/// <summary>
		/// Downloads the contents of the project template.
		/// </summary>
		public async Task<IArchive> DownloadTemplateContentsAsync(Project project)
		{
			return await _repoClient.GetRepositoryContentsAsync
			(
				project.Classroom.GitHubOrganization,
				$"{project.Name}_Template",
				null /*branchName*/,
				ArchiveStore.Memory
			);
		}

		/// <summary>
		/// Downloads submissions for a set of students.
		/// </summary>
		public async Task<StudentSubmissions> DownloadSubmissionsAsync(
			Checkpoint checkpoint,
			IList<StudentDownloadRequest> studentDownloadRequests)
		{
			var orgName = checkpoint.Project.Classroom.GitHubOrganization;
			var projName = checkpoint.Project.Name;

			var students = studentDownloadRequests
				.Select(request => request.Student)
				.ToList();

			var studentsWithSubmissions = new HashSet<ClassroomMembership>
			(
				studentDownloadRequests
					.Where(request => request.Submitted)
					.Select(request => request.Student)
			);

			var studentRepos = await _repoMetadataRetriever.GetStudentRepositoriesAsync
			(
				checkpoint.Project,
				students
			);

			var submissions = await _operationRunner.RunOperationsAsync
			(
				studentRepos.Keys,
				async student => new StudentSubmission
				(
					student,
					await _repoClient.GetRepositoryContentsAsync
					(
						orgName,
						_repoMetadataRetriever.GetRepoName(checkpoint.Project, student),
						studentsWithSubmissions.Contains(student)
							? checkpoint.Name
							: null,
						ArchiveStore.FileSystem
					)
				)
			);

			return new StudentSubmissions(submissions);
		}
	}
}
