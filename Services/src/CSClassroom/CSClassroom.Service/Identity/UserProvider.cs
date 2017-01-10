using System.Threading.Tasks;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Identity
{
	/// <summary>
	/// Provides the current user.
	/// </summary>
	public class UserProvider : IUserProvider
	{
		/// <summary>
		/// The user service.
		/// </summary>
		private readonly IUserService _userService;

		/// <summary>
		/// The identity provider.
		/// </summary>
		private readonly IIdentityProvider _identityProvider;

		/// <summary>
		/// Whether or not the user provider has been initialized.
		/// </summary>
		private bool _initialized;
		
		/// <summary>
		/// The current user.
		/// </summary>
		private User _currentUser;

		/// <summary>
		/// Return whether or not there are any registered users.
		/// </summary>
		private bool _anyRegisteredUsers;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserProvider(IUserService userService, IIdentityProvider identityProvider)
		{
			_userService = userService;
			_identityProvider = identityProvider;
		}

		/// <summary>
		/// Returns the current identity state for the request.
		/// </summary>
		public async Task<IdentityState> GetCurrentIdentityStateAsync()
		{
			await EnsureInitializedAsync();

			if (_identityProvider.CurrentIdentity == null)
				return IdentityState.Anonymous;
			
			if (_currentUser == null)
				return IdentityState.Unregistered;

			return _currentUser.SuperUser
				? IdentityState.SuperUser
				: IdentityState.Registered;
		}

		/// <summary>
		/// Returns the current user for this request.
		/// </summary>
		/// <returns></returns>
		public async Task<User> GetCurrentUserAsync()
		{
			await EnsureInitializedAsync();

			return _currentUser;
		}

		/// <summary>
		/// Return whether or not any users have been registered.
		/// </summary>
		public async Task<bool> IsServiceActivatedAsync()
		{
			await EnsureInitializedAsync();

			return _anyRegisteredUsers;
		}

		/// <summary>
		/// Ensures that the user provider is initialized.
		/// </summary>
		private async Task EnsureInitializedAsync()
		{
			if (_initialized)
			{
				return;
			}

			if (_identityProvider.CurrentIdentity != null)
			{
				_currentUser = await _userService.GetAndUpdateCurrentUserAsync();
			}

			_anyRegisteredUsers =
				   _currentUser != null
				|| await _userService.AnyRegisteredUsersAsync();

			_initialized = true;
		}
	}
}
