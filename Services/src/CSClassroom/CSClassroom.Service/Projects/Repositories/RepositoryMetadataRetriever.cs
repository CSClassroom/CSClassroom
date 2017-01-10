using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Repositories
{
	/// <summary>
	/// Retrieves metadata about project repositories.
	/// </summary>
	public class RepositoryMetadataRetriever : IRepositoryMetadataRetriever
	{
		/// <summary>
		/// The GitHub repository client.
		/// </summary>
		private readonly IGitHubRepositoryClient _repoClient;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RepositoryMetadataRetriever(IGitHubRepositoryClient repoClient)
		{
			_repoClient = repoClient;
		}

		/// <summary>
		/// Retrieves a list of student repositories for a given project.
		/// </summary>
		public async Task<IDictionary<ClassroomMembership, GitHubRepository>> GetStudentRepositoriesAsync(
			Project project,
			IList<ClassroomMembership> students)
		{
			var orgName = project.Classroom.GitHubOrganization;
			var repoList = await _repoClient.GetAllRepositoriesAsync(orgName);
			var repoDictionary = repoList.ToDictionary
			(
				repo => repo.Name,
				repo => repo
			);

			return students
				.Where
				(
					student => repoDictionary.ContainsKey
					(
						GetRepoName(project, student)
					)
				)
				.ToDictionary
				(
					student => student,
					student => repoDictionary[GetRepoName(project, student)]
				);
		}

		/// <summary>
		/// Returns the repository name for the given student/project.
		/// </summary>
		public string GetRepoName(Project project, ClassroomMembership student)
		{
			return $"{project.Name}_{student.GitHubTeam}";
		}
	}
}
