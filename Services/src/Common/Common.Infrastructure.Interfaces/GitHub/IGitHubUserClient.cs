using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Performs user-related operations.
	/// </summary>
	public interface IGitHubUserClient
	{
		/// <summary>
		/// Checks if a given user exists.
		/// </summary>
		Task<bool> DoesUserExistAsync(string userName);
	}
}
