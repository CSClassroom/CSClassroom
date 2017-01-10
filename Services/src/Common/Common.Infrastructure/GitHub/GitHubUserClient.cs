using System.Threading.Tasks;
using Octokit;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Performs user-related operations.
	/// </summary>
	public class GitHubUserClient : IGitHubUserClient
	{
		/// <summary>
		/// The GitHub client.
		/// </summary>
		private readonly GitHubClient _client;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubUserClient(GitHubClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Checks if a given user exists.
		/// </summary>
		public async Task<bool> DoesUserExistAsync(string userName)
		{
			try
			{
				return await _client.User.Get(userName) != null;
			}
			catch (NotFoundException)
			{
				return false;
			}
		}
	}
}
