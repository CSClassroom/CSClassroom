using System.Threading.Tasks;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Identity
{
	/// <summary>
	/// The current identity state.
	/// </summary>
	public enum IdentityState
	{
		Anonymous,
		Unregistered,
		Registered,
		SuperUser
	}

	/// <summary>
	/// Provides information about the current user.
	/// </summary>
	public interface IUserProvider
	{
		/// <summary>
		/// Returns the current identity state for the request.
		/// </summary>
		Task<IdentityState> GetCurrentIdentityStateAsync();

		/// <summary>
		/// Returns the current registered user for the request (if any).
		/// </summary>
		Task<User> GetCurrentUserAsync();

		/// <summary>
		/// Return whether or not any users have been registered.
		/// </summary>
		Task<bool> IsServiceActivatedAsync();
	}
}
