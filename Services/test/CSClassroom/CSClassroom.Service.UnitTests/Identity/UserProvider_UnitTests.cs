using System.Threading.Tasks;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Identity;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Identity
{
	/// <summary>
	/// Unit tests for the UserProvider class.
	/// </summary>
	public class UserProvider_UnitTests
	{
		/// <summary>
		/// Ensures that GetCurrentIdentityStateAsync returns Anonymous for
		/// a user that is not signed in.
		/// </summary>
		[Fact]
		public async Task GetCurrentIdentityStateAsync_NotSignedIn()
		{
			var identityProvider = new Mock<IIdentityProvider>();
			identityProvider
				.Setup(ip => ip.CurrentIdentity)
				.Returns<Model.Users.Identity>(null);

			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.AnyRegisteredUsersAsync())
				.ReturnsAsync(true);

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var identityState = await userProvider.GetCurrentIdentityStateAsync();

			Assert.Equal(IdentityState.Anonymous, identityState);
		}

		/// <summary>
		/// Ensures that GetCurrentIdentityStateAsync returns Unregistered for
		/// a signed-in user that is not registered.
		/// </summary>
		[Fact]
		public async Task GetCurrentIdentityStateAsync_Unregistered()
		{
			var identityProvider = new Mock<IIdentityProvider>();
			identityProvider
				.Setup(ip => ip.CurrentIdentity)
				.Returns(GetIdentity());

			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync((User)null);

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var identityState = await userProvider.GetCurrentIdentityStateAsync();

			Assert.Equal(IdentityState.Unregistered, identityState);
		}

		/// <summary>
		/// Ensures that GetCurrentIdentityStateAsync returns Registered for
		/// a registered user.
		/// </summary>
		[Fact]
		public async Task GetCurrentIdentityStateAsync_Registered()
		{
			var identityProvider = new Mock<IIdentityProvider>();
			identityProvider
				.Setup(ip => ip.CurrentIdentity)
				.Returns(GetIdentity());

			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync(new User());

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var identityState = await userProvider.GetCurrentIdentityStateAsync();

			Assert.Equal(IdentityState.Registered, identityState);
		}

		/// <summary>
		/// Ensures that GetCurrentIdentityStateAsync returns Registered for
		/// a registered superuser.
		/// </summary>
		[Fact]
		public async Task GetCurrentIdentityStateAsync_SuperUser()
		{
			var identityProvider = new Mock<IIdentityProvider>();
			identityProvider
				.Setup(ip => ip.CurrentIdentity)
				.Returns(GetIdentity());

			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync(new User() { SuperUser = true });

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var identityState = await userProvider.GetCurrentIdentityStateAsync();

			Assert.Equal(IdentityState.SuperUser, identityState);
		}

		/// <summary>
		/// Ensures that GetCurrentUserAsync returns null if the user
		/// is not registered.
		/// </summary>
		[Fact]
		public async Task GetCurrentUserAsync_Unregistered_ReturnsNull()
		{
			var identityProvider = new Mock<IIdentityProvider>();

			var user = new User();
			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync((User)null);

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var currentUser = await userProvider.GetCurrentUserAsync();

			Assert.Null(currentUser);
		}

		/// <summary>
		/// Ensures that GetCurrentUserAsync returns the current registered user.
		/// </summary>
		[Fact]
		public async Task GetCurrentUserAsync_Registered_ReturnsUser()
		{
			var identityProvider = new Mock<IIdentityProvider>();
			identityProvider
				.Setup(ip => ip.CurrentIdentity)
				.Returns(GetIdentity());

			var user = new User();
			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync(user);

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var currentUser = await userProvider.GetCurrentUserAsync();

			Assert.Equal(user, currentUser);
		}

		/// <summary>
		/// Ensures that IsServiceActivatedAsync returns false when there
		/// are no registered users, for an anonymous user.
		/// </summary>
		[Fact]
		public async Task IsServiceActivatedAsync_NotSignedInAndNoRegisteredUsers_ReturnsFalse()
		{
			var identityProvider = new Mock<IIdentityProvider>();

			var user = new User();
			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync((User)null);
			userService
				.Setup(us => us.AnyRegisteredUsersAsync())
				.ReturnsAsync(false);

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var result = await userProvider.IsServiceActivatedAsync();

			Assert.False(result);
		}

		/// <summary>
		/// Ensures that IsServiceActivatedAsync returns true when there
		/// are registered users, for an anonymous user.
		/// </summary>
		[Fact]
		public async Task IsServiceActivatedAsync_NotSignedInAndRegisteredUsers_ReturnsTrue()
		{
			var identityProvider = new Mock<IIdentityProvider>();

			var user = new User();
			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync((User)null);
			userService
				.Setup(us => us.AnyRegisteredUsersAsync())
				.ReturnsAsync(true);

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var result = await userProvider.IsServiceActivatedAsync();

			Assert.True(result);
		}

		/// <summary>
		/// Ensures that IsServiceActivatedAsync returns true when the user
		/// is signed in as a registered user.
		/// </summary>
		[Fact]
		public async Task IsServiceActivatedAsync_SignedInAsRegisteredUser_ReturnsTrue()
		{
			var identityProvider = new Mock<IIdentityProvider>();
			identityProvider
				.Setup(ip => ip.CurrentIdentity)
				.Returns(GetIdentity());

			var user = new User();
			var userService = new Mock<IUserService>();
			userService
				.Setup(us => us.GetAndUpdateCurrentUserAsync())
				.ReturnsAsync(new User());

			var userProvider = new UserProvider(userService.Object, identityProvider.Object);
			var result = await userProvider.IsServiceActivatedAsync();

			Assert.True(result);
			userService.Verify(us => us.AnyRegisteredUsersAsync(), Times.Never);
		}

		/// <summary>
		/// Returns an identity.
		/// </summary>
		private Model.Users.Identity GetIdentity()
		{
			return new Model.Users.Identity
			(
				"UniqueId",
				"UserName",
				"FirstName",
				"LastName"
			);
		}
	}
}
