using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Performs organization-related operations.
	/// </summary>
	public interface IGitHubOrganizationClient
	{
		/// <summary>
		/// Checks if a given member belongs to a given organization.
		/// </summary>
		Task<bool> CheckMemberAsync(string organizationName, string userName);
	}
}
