using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Identity
{
	/// <summary>
	/// Unit tests for the UserService.
	/// </summary>
	public class UserService_UnitTests
	{
		/// <summary>
		/// Ensures that GetAndUpdateCurrentUserAsync returns null 
		/// for an anonymous user.
		/// </summary>
		[Fact]
		public async Task GetAndUpdateCurrentUserAsync_Anonymous_ReturnsNull()
		{
			var identityProvider = GetMockIdentityProvider(identity: null);

			var userService = GetUserService
			(
				null /*dbContext*/, 
				identityProvider.Object
			);

			var currentUser = await userService.GetAndUpdateCurrentUserAsync();

			Assert.Null(currentUser);
		}

		/// <summary>
		/// Ensures that GetAndUpdateCurrentUserAsync returns null 
		/// for an unregistered user.
		/// </summary>
		[Fact]
		public async Task GetAndUpdateCurrentUserAsync_Unregistered_ReturnsNull()
		{
			var database = new TestDatabaseBuilder().Build();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var currentUser = await userService.GetAndUpdateCurrentUserAsync();

			Assert.Null(currentUser);
		}

		/// <summary>
		/// Ensures that GetAndUpdateCurrentUserAsync returns the registered user
		/// that is currently signed in.
		/// </summary>
		[Fact]
		public async Task GetAndUpdateCurrentUserAsync_Registered_ReturnsUser()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
			   .Build();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var currentUser = await userService.GetAndUpdateCurrentUserAsync();
			var classroomMembership = currentUser.ClassroomMemberships.Single();
			var sectionMembership = classroomMembership.SectionMemberships.Single();

			Assert.Equal("User1", currentUser.UserName);
			Assert.Equal("Class1", classroomMembership.Classroom.Name);
			Assert.Equal("Section1", sectionMembership.Section.Name);
		}

		/// <summary>
		/// Ensures that GetAndUpdateCurrentUserAsync updates the username
		/// for a given unique ID, if the username changed.
		/// </summary>
		[Fact]
		public async Task GetAndUpdateCurrentUserAsync_UserNameChanged_UserNameUpdated()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
			   .Build();

			var identityProvider = GetMockIdentityProvider(GetIdentity("NewUser1", "User1Id"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var currentUser = await userService.GetAndUpdateCurrentUserAsync();
			var classroomMembership = currentUser.ClassroomMemberships.Single();
			var sectionMembership = classroomMembership.SectionMemberships.Single();

			Assert.Equal("NewUser1", currentUser.UserName);
			Assert.Equal("Class1", classroomMembership.Classroom.Name);
			Assert.Equal("Section1", sectionMembership.Section.Name);

			database.Reload();

			currentUser = database.Context.Users.First();
			Assert.Equal("NewUser1", currentUser.UserName);
		}

		/// <summary>
		/// Ensures that GetAndUpdateCurrentUserAsync checks GitHub to see
		/// if the user has joined the classroom's GitHub oranization, if
		/// the user is not yet known to be in the organization.
		/// </summary>
		[Fact]
		public async Task GetAndUpdateCurrentUserAsync_UserNotInOrg_OrgMembershipChecked()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser")
			   .Build();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubOrgClient = GetMockGitHubOrgClient(userInOrg: false);

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubOrgClient: gitHubOrgClient.Object
			);

			var currentUser = await userService.GetAndUpdateCurrentUserAsync();
			var classroomMembership = currentUser.ClassroomMemberships.Single();
			
			Assert.False(classroomMembership.InGitHubOrganization);

			gitHubOrgClient.Verify
			(
				gc => gc.CheckMemberAsync("Class1GitHubOrg", "GitHubUser"), 
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that GetAndUpdateCurrentUserAsync updates a user that just
		/// joined the class's GitHub organization.
		/// </summary>
		[Fact]
		public async Task GetAndUpdateCurrentUserAsync_UserJustJoinedOrg_OrgMembershipUpdated()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser")
			   .Build();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubOrgClient = GetMockGitHubOrgClient(userInOrg: true);

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubOrgClient: gitHubOrgClient.Object
			);

			var currentUser = await userService.GetAndUpdateCurrentUserAsync();
			var classroomMembership = currentUser.ClassroomMemberships.Single();

			Assert.True(classroomMembership.InGitHubOrganization);

			database.Reload();

			currentUser = database.Context
				.Users.Include(u => u.ClassroomMemberships)
				.First();

			classroomMembership = currentUser.ClassroomMemberships.Single();

			Assert.True(classroomMembership.InGitHubOrganization);
		}

		/// <summary>
		/// Ensures that GetAndUpdateCurrentUserAsync does not ask GitHub about
		/// a user's organization membershp, if the user is already known to be
		/// in the organization.
		/// </summary>
		[Fact]
		public async Task GetAndUpdateCurrentUserAsync_UserAlreadyInOrg_GitHubNotCalled()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
			   .Build();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubOrgClient = new Mock<IGitHubOrganizationClient>();

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubOrgClient: gitHubOrgClient.Object
			);

			var currentUser = await userService.GetAndUpdateCurrentUserAsync();
			var classroomMembership = currentUser.ClassroomMemberships.Single();

			Assert.True(classroomMembership.InGitHubOrganization);
			
			gitHubOrgClient.Verify
			(
				gc => gc.CheckMemberAsync(It.IsAny<string>(), It.IsAny<string>()),
				Times.Never
			);
		}

		/// <summary>
		/// Ensures that GetUserAsync returns the user with the given id.
		/// </summary>
		[Fact]
		public async Task GetUserAsync_ReturnsUser()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
			   .AddAdditionalContact("User1", "LastName2", "FirstName2", "EmailAddress2")
			   .Build();

			var userId = database.Context.Users.Single().Id;

			database.Reload();

			var userService = GetUserService(database.Context);

			var currentUser = await userService.GetUserAsync(userId);
			var classroomMembership = currentUser.ClassroomMemberships.Single();
			var sectionMembership = classroomMembership.SectionMemberships.Single();
			var additionalContact = currentUser.AdditionalContacts.Single();

			Assert.Equal("User1", currentUser.UserName);
			Assert.Equal("Class1", classroomMembership.Classroom.Name);
			Assert.Equal("Section1", sectionMembership.Section.Name);
			Assert.Equal("FirstName2", additionalContact.FirstName);
			Assert.Equal("LastName2", additionalContact.LastName);
			Assert.Equal("EmailAddress2", additionalContact.EmailAddress);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will not update the user if they enter
		/// a GitHub username that does not exist.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_InvalidGitHubUser_UserNotUpdated()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "OldGitHubUser")
			   .Build();

			var user = database.Context.Users.Single();
			database.Reload();

			user.GitHubLogin = "GitHubUser";

			var modelErrors = new MockErrorCollection();
			var userClient = GetMockGitHubUserClient(userExists: false);
			var userService = GetUserService
			(
				database.Context,
				gitHubUserClient: userClient.Object
			);

			var result = await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users.Single();

			Assert.False(result);
			Assert.True(modelErrors.HasErrors);
			Assert.True(modelErrors.VerifyErrors("GitHubLogin"));
			Assert.Equal("OldGitHubUser", user.GitHubLogin);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will remove the old user from the GitHub
		/// team and add the new user to the GitHub team, when the GitHub username
		/// has changed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_ValidGitHubUser_UserOnTeamUpdated()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "OldGitHubUser", inGitHubOrg: true)
			   .Build();

			var user = database.Context.Users.Single();
			database.Reload();

			user.GitHubLogin = "GitHubUser";

			var modelErrors = new MockErrorCollection();
			var userClient = GetMockGitHubUserClient(userExists: true);
			var teamClient = GetMockGitHubTeamClient("Class1");
			var userService = GetUserService
			(
				database.Context,
				gitHubUserClient: userClient.Object,
				gitHubTeamClient: teamClient.Object
			);

			var result = await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users
				.Include(u => u.ClassroomMemberships)
				.Single();

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);
			Assert.Equal("GitHubUser", user.GitHubLogin);
			Assert.False(user.ClassroomMemberships.Single().InGitHubOrganization);

			teamClient.Verify
			(
				tc => tc.RemoveUserFromTeamAsync
				(
					"Class1GitHubOrg",
					It.Is<GitHubTeam>(t => t.Name == "LastNameFirstName"),
					"OldGitHubUser"
				),
				Times.Once
			);

			teamClient.Verify
			(
				tc => tc.InviteUserToTeamAsync
				(
					"Class1GitHubOrg", 
					It.Is<GitHubTeam>(t => t.Name == "LastNameFirstName"), 
					"GitHubUser"
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will send a new confirmation e-mail with a new
		/// confirmation code (and mark the account as unconfirmed), when the e-mail
		/// address is changed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_EmailAddressUpdated_ConfirmationEmailSent()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
			   .Build();

			var user = database.Context.Users.Single();
			user.EmailAddressConfirmed = true;
			database.Context.SaveChanges();
			database.Reload();

			user.EmailAddress = "NewEmailAddress";

			var modelErrors = new MockErrorCollection();
			var emailProvider = GetMockEmailProvider("NewEmailAddress");
			var userService = GetUserService
			(
				database.Context,
				emailProvider: emailProvider.Object
			);

			var result = await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users.Single();

			Assert.True(result);
			Assert.True(user.EmailConfirmationCode != "User1EmailConfirmationCode");
			Assert.False(user.EmailAddressConfirmed);
			emailProvider.Verify
			(
				ep => ep.SendMessageAsync
				(
					It.Is<IList<EmailRecipient>>
					(
						to => to.Count == 1 && to[0].EmailAddress == "NewEmailAddress"
					),
					It.IsAny<string>(),
					It.IsAny<string>()
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will send a new confirmation e-mail with a new
		/// confirmation code (and mark the account as unconfirmed), when the e-mail
		/// address is changed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_EmailAddressNotUpdated_NoConfirmationEmailSent()
		{
			var database = new TestDatabaseBuilder()
			   .AddClassroom("Class1")
			   .AddSection("Class1", "Section1")
			   .AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
			   .Build();

			var user = database.Context.Users.Single();
			user.EmailAddressConfirmed = true;
			database.Context.SaveChanges();
			database.Reload();

			var modelErrors = new MockErrorCollection();
			var emailProvider = GetMockEmailProvider("User1Email");
			var userService = GetUserService
			(
				database.Context,
				emailProvider: emailProvider.Object
			);

			await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users.Single();
			
			Assert.Equal("User1EmailConfirmationCode", user.EmailConfirmationCode);
			Assert.True(user.EmailAddressConfirmed);
			emailProvider.Verify
			(
				ep => ep.SendMessageAsync
				(
					It.Is<IList<EmailRecipient>>
					(
						to => to.Count == 1 && to[0].EmailAddress == "User1Email"
					),
					It.IsAny<string>(),
					It.IsAny<string>()
				),
				Times.Never
			);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will update the user's public name if it 
		/// has changed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_PublicNameUpdated_ChangeSaved()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var user = database.Context.Users.Single();
			user.EmailAddressConfirmed = true;
			database.Context.SaveChanges();
			database.Reload();

			user.PublicName = "NewPublicName";

			var modelErrors = new MockErrorCollection();
			var userService = GetUserService(database.Context);

			var result = await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users.Single();

			Assert.True(result);
			Assert.Equal("NewPublicName", user.PubliclyDisplayedName);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will update the user's public name if it 
		/// has changed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_AdditionalContactAdded_ChangeSaved()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var user = database.Context.Users.Single();
			user.EmailAddressConfirmed = true;
			database.Context.SaveChanges();
			database.Reload();

			user.AdditionalContacts = new List<AdditionalContact>
			{
				new AdditionalContact()
				{
					LastName = "LastName2",
					FirstName = "FirstName2",
					EmailAddress = "EmailAddress2"
				}
			};

			var modelErrors = new MockErrorCollection();
			var userService = GetUserService(database.Context);

			var result = await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users.Include(u => u.AdditionalContacts).Single();

			Assert.True(result);
			Assert.Equal("LastName2", user.AdditionalContacts.Single().LastName);
			Assert.Equal("FirstName2", user.AdditionalContacts.Single().FirstName);
			Assert.Equal("EmailAddress2", user.AdditionalContacts.Single().EmailAddress);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will update the user's public name if it 
		/// has changed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_AdditionalContactModified_ChangeSaved()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.AddAdditionalContact("User1", "LastName2", "FirstName2", "EmailAddress2")
				.Build();
			
			var user = database.Context.Users.Include(u => u.AdditionalContacts).Single();
			user.EmailAddressConfirmed = true;
			database.Context.SaveChanges();
			database.Reload();

			user.AdditionalContacts[0].LastName = "LastName3";
			user.AdditionalContacts[0].FirstName = "FirstName3";
			user.AdditionalContacts[0].EmailAddress = "EmailAddress3";

			var modelErrors = new MockErrorCollection();
			var userService = GetUserService(database.Context);

			var result = await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users.Include(u => u.AdditionalContacts).Single();

			Assert.True(result);
			Assert.Equal("LastName3", user.AdditionalContacts.Single().LastName);
			Assert.Equal("FirstName3", user.AdditionalContacts.Single().FirstName);
			Assert.Equal("EmailAddress3", user.AdditionalContacts.Single().EmailAddress);
		}

		/// <summary>
		/// Ensures that UpdateUserAsync will update the user's public name if it 
		/// has changed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_AdditionalContactRemoved_ChangeSaved()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.AddAdditionalContact("User1", "LastName2", "FirstName2", "EmailAddress2")
				.Build();
			
			var user = database.Context.Users.Include(u => u.AdditionalContacts).Single();
			user.EmailAddressConfirmed = true;
			database.Context.SaveChanges();
			database.Reload();

			user.AdditionalContacts.Clear();

			var modelErrors = new MockErrorCollection();
			var userService = GetUserService(database.Context);

			var result = await userService.UpdateUserAsync
			(
				user,
				"ConfirmationUrlBuilder",
				modelErrors
			);

			database.Reload();
			user = database.Context.Users.Include(u => u.AdditionalContacts).Single();

			Assert.True(result);
			Assert.Empty(user.AdditionalContacts);
		}

		/// <summary>
		/// Ensures that AnyRegisteredUsersAsync returns false if there
		/// are no registered users.
		/// </summary>
		[Fact]
		public async Task AnyRegisteredUsersAsync_No_ReturnsFalse()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			var userService = GetUserService(database.Context);

			var result = await userService.AnyRegisteredUsersAsync();

			Assert.False(result);
		}

		/// <summary>
		/// Ensures that AnyRegisteredUsersAsync returns true if there
		/// are registered users.
		/// </summary>
		[Fact]
		public async Task AnyRegisteredUsersAsync_Yes_ReturnsTrue()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var userService = GetUserService(database.Context);

			var result = await userService.AnyRegisteredUsersAsync();

			Assert.True(result);
		}

		/// <summary>
		/// Ensures that RegisterFirstSuperUserAsync does not register
		/// a super user if any users currently exist.
		/// </summary>
		[Fact]
		public async Task RegisterFirstSuperUserAsync_ExistingUser_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var modelErrors = new MockErrorCollection();
			var userService = GetUserService(database.Context);

			var result = await userService.RegisterFirstSuperUserAsync
			(
				new SuperUserRegistration(),
				modelErrors
			);

			Assert.Equal(RegisterNewUserResult.AlreadyRegistered, result);
			Assert.Equal(1, database.Context.Users.Count());
		}

		/// <summary>
		/// Ensures that RegisterFirstSuperUserAsync does not register
		/// a super user if the activation token does not match.
		/// </summary>
		[Fact]
		public async Task RegisterFirstSuperUserAsync_WrongActivationToken_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder().Build();

			var modelErrors = new MockErrorCollection();
			var activationToken = new ActivationToken("ActivationToken");

			var userService = GetUserService
			(
				database.Context,
				activationToken: activationToken
			);

			var result = await userService.RegisterFirstSuperUserAsync
			(
				new SuperUserRegistration() { ActivationToken = "Wrong" },
				modelErrors
			);

			Assert.Equal(RegisterNewUserResult.Failed, result);
			Assert.True(modelErrors.VerifyErrors("ActivationToken"));
			Assert.Equal(0, database.Context.Users.Count());
		}

		/// <summary>
		/// Ensures that RegisterFirstSuperUserAsync does not register
		/// a super user if their GitHub username does not exist.
		/// </summary>
		[Fact]
		public async Task RegisterFirstSuperUserAsync_NonExistentGitHubUser_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder().Build();

			var modelErrors = new MockErrorCollection();
			var activationToken = new ActivationToken("ActivationToken");
			var gitHubUserClient = GetMockGitHubUserClient(userExists: false);

			var userService = GetUserService
			(
				database.Context,
				activationToken: activationToken,
				gitHubUserClient: gitHubUserClient.Object
			);

			var result = await userService.RegisterFirstSuperUserAsync
			(
				new SuperUserRegistration()
				{
					ActivationToken = "ActivationToken",
					GitHubLogin = "GitHubUser"
				},
				modelErrors
			);

			Assert.Equal(RegisterNewUserResult.Failed, result);
			Assert.True(modelErrors.VerifyErrors("GitHubLogin"));
			Assert.Equal(0, database.Context.Users.Count());
		}

		/// <summary>
		/// Ensures that RegisterFirstSuperUserAsync registers the superuser.
		/// </summary>
		[Fact]
		public async Task RegisterFirstSuperUserAsync_ValidRegistration_Successful()
		{
			var database = new TestDatabaseBuilder().Build();

			var modelErrors = new MockErrorCollection();
			var activationToken = new ActivationToken("ActivationToken");
			var gitHubUserClient = GetMockGitHubUserClient(userExists: true);
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context, 
				identityProvider.Object, 
				gitHubUserClient.Object, 
				activationToken: activationToken
			);

			var result = await userService.RegisterFirstSuperUserAsync
			(
				new SuperUserRegistration()
				{
					LastName = "LastName",
					FirstName = "FirstName",
					ActivationToken = "ActivationToken",
					GitHubLogin = "GitHubUser",
					EmailAddress = "EmailAddress"
				},
				modelErrors
			);

			database.Reload();

			var user = database.Context.Users.Single();

			Assert.Equal(RegisterNewUserResult.Success, result);
			Assert.False(modelErrors.HasErrors);
			Assert.Equal(1, database.Context.Users.Count());
			Assert.Equal("LastName", user.LastName);
			Assert.Equal("FirstName", user.FirstName);
			Assert.Equal("User1Id", user.UniqueId);
			Assert.Equal("User1", user.UserName);
			Assert.Equal("GitHubUser", user.GitHubLogin);
			Assert.Equal("EmailAddress", user.EmailAddress);
			Assert.True(user.EmailAddressConfirmed);
			Assert.True(user.SuperUser);
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync does not register the student
		/// if the section is not found.
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_SectionNotFound_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			var modelErrors = new MockErrorCollection();
			var userService = GetUserService(database.Context);

			var result = await userService.RegisterNewStudentAsync
			(
				"Class1",
				"NonExistentSection",
				new StudentRegistration(),
				"ConfirmationUrl",
				modelErrors
			);

			Assert.Equal(RegisterNewUserResult.SectionNotFound, result);
			Assert.Equal(0, database.Context.Users.Count());
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync does not register the student
		/// if the section is not open for registration.
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_SectionNotOpen_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1", allowRegistrations: false)
				.Build();

			var modelErrors = new MockErrorCollection();
			var userService = GetUserService(database.Context);

			var result = await userService.RegisterNewStudentAsync
			(
				"Class1",
				"Section1",
				new StudentRegistration(),
				"ConfirmationUrl",
				modelErrors
			);

			Assert.Equal(RegisterNewUserResult.SectionNotOpen, result);
			Assert.Equal(0, database.Context.Users.Count());
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync does not register the student
		/// if they are already registered.
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_AlreadyRegistered_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var modelErrors = new MockErrorCollection();
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context, 
				identityProvider.Object
			);

			var result = await userService.RegisterNewStudentAsync
			(
				"Class1",
				"Section1",
				new StudentRegistration(),
				"ConfirmationUrl",
				modelErrors
			);

			Assert.Equal(RegisterNewUserResult.AlreadyRegistered, result);
			Assert.Equal(1, database.Context.Users.Count());
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync does not register the student
		/// if their GitHub username does not exist.
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_NonExistentGitHubUser_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();


			var modelErrors = new MockErrorCollection();
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubUserClient = GetMockGitHubUserClient(userExists: false);

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubUserClient.Object
			);

			var result = await userService.RegisterNewStudentAsync
			(
				"Class1",
				"Section1",
				new StudentRegistration() { GitHubLogin = "GitHubUser" },
				"ConfirmationUrl",
				modelErrors
			);

			Assert.Equal(RegisterNewUserResult.Failed, result);
			Assert.True(modelErrors.VerifyErrors("GitHubLogin"));
			Assert.Equal(0, database.Context.Users.Count());
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync registers a new student,
		/// and returns the result.
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_ValidRegistration_ReturnsResult()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			var modelErrors = new MockErrorCollection();
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubUserClient = GetMockGitHubUserClient(userExists: true);
			var gitHubTeamClient = GetMockGitHubTeamClient("Class1");
			var emailProvider = GetMockEmailProvider("EmailAddress");

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubUserClient.Object,
				gitHubTeamClient: gitHubTeamClient.Object,
				emailProvider: emailProvider.Object
			);

			var result = await userService.RegisterNewStudentAsync
			(
				"Class1",
				"Section1",
				new StudentRegistration()
				{
					FirstName = "FirstName",
					GitHubLogin = "GitHubUser",
					EmailAddress = "EmailAddress",
				},
				"ConfirmationUrl",
				modelErrors
			);

			database.Reload();

			var student = database.Context.Users
				.Include(u => u.ClassroomMemberships)
					.ThenInclude(cm => cm.Classroom)
				.Include(u => u.ClassroomMemberships)
					.ThenInclude(cm => cm.SectionMemberships)
						.ThenInclude(sm => sm.Section)
				.Single();

			var classroomMembership = student.ClassroomMemberships.Single();
			var sectionMembership = classroomMembership.SectionMemberships.Single();

			Assert.Equal(RegisterNewUserResult.Success, result);
			Assert.False(modelErrors.HasErrors);
			Assert.Equal(1, database.Context.Users.Count());
			Assert.Equal("LastName", student.LastName);
			Assert.Equal("FirstName", student.FirstName);
			Assert.Equal("User1Id", student.UniqueId);
			Assert.Equal("User1", student.UserName);
			Assert.Equal("GitHubUser", student.GitHubLogin);
			Assert.Equal("EmailAddress", student.EmailAddress);
			Assert.False(student.EmailAddressConfirmed);
			Assert.False(student.SuperUser);
			Assert.NotNull(student.EmailConfirmationCode);
			Assert.Equal("Class1", classroomMembership.Classroom.Name);
			Assert.Equal("Section1", sectionMembership.Section.Name);
			Assert.Equal("LastNameFirstName", classroomMembership.GitHubTeam);
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync registers a new student and
		/// returns the result, even when their first and last names are the
		/// same as another student. 
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_ValidRegistrationDuplicateName_UniqueGitHubTeam()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var modelErrors = new MockErrorCollection();
			var identityProvider = GetMockIdentityProvider(GetIdentity("User2"));
			var gitHubUserClient = GetMockGitHubUserClient(userExists: true);
			var gitHubTeamClient = GetMockGitHubTeamClient("Class1");
			var emailProvider = GetMockEmailProvider("EmailAddress");

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubUserClient.Object,
				gitHubTeamClient: gitHubTeamClient.Object,
				emailProvider: emailProvider.Object
			);

			var result = await userService.RegisterNewStudentAsync
			(
				"Class1",
				"Section1",
				new StudentRegistration()
				{
					FirstName = "FirstName",
					GitHubLogin = "GitHubUser",
					EmailAddress = "EmailAddress",
				},
				"ConfirmationUrl",
				modelErrors
			);

			database.Reload();

			var student = database.Context.Users
				.Include(u => u.ClassroomMemberships)
					.ThenInclude(cm => cm.Classroom)
				.Include(u => u.ClassroomMemberships)
					.ThenInclude(cm => cm.SectionMemberships)
						.ThenInclude(sm => sm.Section)
				.Single(u => u.UserName == "User2");

			var classroomMembership = student.ClassroomMemberships.Single();
			var sectionMembership = classroomMembership.SectionMemberships.Single();

			Assert.Equal(RegisterNewUserResult.Success, result);
			Assert.False(modelErrors.HasErrors);
			Assert.Equal(2, database.Context.Users.Count());
			Assert.Equal("LastName", student.LastName);
			Assert.Equal("FirstName", student.FirstName);
			Assert.Equal("User2Id", student.UniqueId);
			Assert.Equal("User2", student.UserName);
			Assert.Equal("GitHubUser", student.GitHubLogin);
			Assert.Equal("EmailAddress", student.EmailAddress);
			Assert.False(student.EmailAddressConfirmed);
			Assert.False(student.SuperUser);
			Assert.NotNull(student.EmailConfirmationCode);
			Assert.Equal("Class1", classroomMembership.Classroom.Name);
			Assert.Equal("Section1", sectionMembership.Section.Name);
			Assert.Equal("LastNameFirstName2", classroomMembership.GitHubTeam);
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync invites the new user to the GitHub team.
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_ValidRegistration_InvitesToTeam()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			var modelErrors = new MockErrorCollection();
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubUserClient = GetMockGitHubUserClient(userExists: true);
			var gitHubTeamClient = GetMockGitHubTeamClient("Class1");
			var emailProvider = GetMockEmailProvider("EmailAddress");

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubUserClient.Object,
				gitHubTeamClient: gitHubTeamClient.Object,
				emailProvider: emailProvider.Object
			);

			await userService.RegisterNewStudentAsync
			(
				"Class1",
				"Section1",
				new StudentRegistration()
				{
					FirstName = "FirstName",
					GitHubLogin = "GitHubUser",
					EmailAddress = "EmailAddress",
				},
				"ConfirmationUrl",
				modelErrors
			);

			gitHubTeamClient.Verify
			(
				gc => gc.InviteUserToTeamAsync
				(
					"Class1GitHubOrg", 
					It.Is<GitHubTeam>(t => t.Name == "LastNameFirstName"), 
					"GitHubUser"
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync sends a confirmation mail to the user
		/// that just registered.
		/// </summary>
		[Fact]
		public async Task RegisterNewStudentAsync_ValidRegistration_SendsConfirmationMail()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			var modelErrors = new MockErrorCollection();
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubUserClient = GetMockGitHubUserClient(userExists: true);
			var gitHubTeamClient = GetMockGitHubTeamClient("Class1");
			var emailProvider = GetMockEmailProvider("EmailAddress");

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubUserClient.Object,
				gitHubTeamClient: gitHubTeamClient.Object,
				emailProvider: emailProvider.Object
			);

			await userService.RegisterNewStudentAsync
			(
				"Class1",
				"Section1",
				new StudentRegistration()
				{
					FirstName = "FirstName",
					GitHubLogin = "GitHubUser",
					EmailAddress = "EmailAddress",
				},
				"ConfirmationUrl",
				modelErrors
			);

			emailProvider.Verify
			(
				ep => ep.SendMessageAsync
				(
					It.Is<IList<EmailRecipient>>
					(
						to => to.Count == 1 && to[0].EmailAddress == "EmailAddress"
					),
					It.IsAny<string>(),
					It.IsAny<string>()
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that RegisterExistingStudentAsync does not register the student
		/// if the section is not found.
		/// </summary>
		[Fact]
		public async Task RegisterExistingStudentAsync_SectionNotFound_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var userService = GetUserService(database.Context);

			var result = await userService.RegisterExistingStudentAsync
			(
				"Class2",
				"NonExistentSection"
			);

			var classroomMemberships = database.Context.Users
				.Include(u => u.ClassroomMemberships)
				.Single().ClassroomMemberships;

			Assert.Equal(RegisterExistingUserResult.SectionNotFound, result);
			Assert.Equal(1, classroomMemberships.Count);
		}

		/// <summary>
		/// Ensures that RegisterExistingStudentAsync does not register the student
		/// if the section is not open for registration.
		/// </summary>
		[Fact]
		public async Task RegisterExistingStudentAsync_SectionNotOpen_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2", allowRegistrations: false)
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();
			
			var userService = GetUserService(database.Context);

			var result = await userService.RegisterExistingStudentAsync
			(
				"Class2",
				"Section2"
			);

			var classroomMemberships = database.Context.Users
				.Include(u => u.ClassroomMemberships)
				.Single().ClassroomMemberships;

			Assert.Equal(RegisterExistingUserResult.SectionNotOpen, result);
			Assert.Equal(1, classroomMemberships.Count);
		}

		/// <summary>
		/// Ensures that RegisterExistingStudentAsync does not register the student
		/// if they are already registered.
		/// </summary>
		[Fact]
		public async Task RegisterExistingStudentAsync_AlreadyRegistered_DoesNotRegister()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();
			
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var result = await userService.RegisterExistingStudentAsync
			(
				"Class1",
				"Section1"
			);

			var classroomMemberships = database.Context.Users
				.Include(u => u.ClassroomMemberships)
				.Single().ClassroomMemberships;

			Assert.Equal(RegisterExistingUserResult.AlreadyRegistered, result);
			Assert.Equal(1, classroomMemberships.Count);
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync registers a new student,
		/// and returns the result.
		/// </summary>
		[Fact]
		public async Task RegisterExistingStudentAsync_ValidRegistration_ReturnsResult()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();
			
			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubTeamClient = GetMockGitHubTeamClient("Class2");

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubTeamClient: gitHubTeamClient.Object
			);

			var result = await userService.RegisterExistingStudentAsync
			(
				"Class2",
				"Section2"
			);

			var classroomMemberships = database.Context.ClassroomMemberships
				.Include(cm => cm.Classroom)
				.OrderBy(cm => cm.Classroom.Name)
				.ToList();

			var sectionMemberships = database.Context.SectionMemberships
				.Include(sm => sm.Section)
				.OrderBy(sm => sm.Section.Name)
				.ToList();

			Assert.Equal(RegisterExistingUserResult.Success, result);
			Assert.Equal(2, classroomMemberships.Count);
			Assert.Equal("Class1", classroomMemberships[0].Classroom.Name);
			Assert.Equal("Class2", classroomMemberships[1].Classroom.Name);
			Assert.Equal("Section1", sectionMemberships[0].Section.Name);
			Assert.Equal(classroomMemberships[0], sectionMemberships[0].ClassroomMembership);
			Assert.Equal("Section2", sectionMemberships[1].Section.Name);
			Assert.Equal(classroomMemberships[1], sectionMemberships[1].ClassroomMembership);
		}

		/// <summary>
		/// Ensures that RegisterNewStudentAsync invites the student to the new classroom's
		/// GitHub organization (even if the student is already a member of another classroom's 
		/// GitHub organization).
		/// </summary>
		[Fact]
		public async Task RegisterExistingStudentAsync_ValidRegistration_InvitesToTeam()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));
			var gitHubTeamClient = GetMockGitHubTeamClient("Class2");

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object,
				gitHubTeamClient: gitHubTeamClient.Object
			);

			await userService.RegisterExistingStudentAsync
			(
				"Class2",
				"Section2"
			);

			gitHubTeamClient.Verify
			(
				gc => gc.InviteUserToTeamAsync
				(
					"Class2GitHubOrg",
					It.Is<GitHubTeam>(t => t.Name == "LastNameFirstName"),
					"GitHubUser"
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that the current user can view themselves.
		/// </summary>
		[Fact]
		public async Task CanViewAndEditUserAsync_CurrentUser_ReturnsTrue()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var userId = database.Context.Users.Single().Id;
			database.Reload();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var result = await userService.CanViewAndEditUserAsync(userId);

			Assert.True(result);
		}

		/// <summary>
		/// Ensures that a class admin can view a user in their class.
		/// </summary>
		[Fact]
		public async Task CanViewAndEditUserAsync_DifferentUserAsClassAdmin_ReturnsTrue()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.AddAdmin("Admin1", "LastName", "FirstName", "Class1", superUser: false)
				.Build();

			var userId = database.Context.Users.Single(u => u.UserName == "User1").Id;
			database.Reload();

			var identityProvider = GetMockIdentityProvider(GetIdentity("Admin1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var result = await userService.CanViewAndEditUserAsync(userId);

			Assert.True(result);
		}

		/// <summary>
		/// Ensures that a superuser can view any user (even a user not in a class they
		/// are an admin of).
		/// </summary>
		[Fact]
		public async Task CanViewAndEditUserAsync_DifferentUserAsSuperUser_ReturnsTrue()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.AddAdmin("Admin1", "LastName", "FirstName", "Class2", superUser: true)
				.Build();

			var userId = database.Context.Users.Single(u => u.UserName == "User1").Id;
			database.Reload();

			var identityProvider = GetMockIdentityProvider(GetIdentity("Admin1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var result = await userService.CanViewAndEditUserAsync(userId);

			Assert.True(result);
		}

		/// <summary>
		/// Ensures that a non-superuser cannot a user in a class for which they
		/// are not an admin.
		/// </summary>
		[Fact]
		public async Task CanViewAndEditUserAsync_UnrelatedUser_ReturnsFalse()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.AddAdmin("Admin1", "LastName", "FirstName", "Class2", superUser: false)
				.Build();

			var userId = database.Context.Users.Single(u => u.UserName == "User1").Id;
			database.Reload();

			var identityProvider = GetMockIdentityProvider(GetIdentity("Admin1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			var result = await userService.CanViewAndEditUserAsync(userId);

			Assert.False(result);
		}

		/// <summary>
		/// Ensures that ResendEmailConfirmationAsync actually sends an e-mail
		/// to the desired user.
		/// </summary>
		[Fact]
		public async Task ResendEmailConfirmationAsync_SendsEmail()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var userId = database.Context.Users.Single().Id;
			database.Reload();

			var emailProvider = GetMockEmailProvider("User1EmailAddress");

			var userService = GetUserService
			(
				database.Context, 
				emailProvider: emailProvider.Object
			);

			await userService.ResendEmailConfirmationAsync(userId, "ConfirmationUrl");

			emailProvider.Verify
			(
				ep => ep.SendMessageAsync
				(
					It.Is<IList<EmailRecipient>>
					(
						to => to.Count == 1 && to[0].EmailAddress == "User1Email"
					),
					It.IsAny<string>(),
					It.IsAny<string>()
				),
				Times.Once
			);
		}
		
		/// <summary>
		/// Ensures that SubmitEmailConfirmationCodeAsync does not mark the user
		/// as confirmed when the confirmation code does not match.
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task SubmitEmailConfirmationCodeAsync_WrongCode_NotConfirmed()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var userId = database.Context.Users.Single().Id;
			database.Reload();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context, 
				identityProvider.Object
			);

			await userService.SubmitEmailConfirmationCodeAsync("WrongCode");

			database.Reload();
			var user = database.Context.Users.Single();

			Assert.False(user.EmailAddressConfirmed);
		}

		/// <summary>
		/// Ensures that SubmitEmailConfirmationCodeAsync does not mark the user
		/// as confirmed when the confirmation code does not match.
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task SubmitEmailConfirmationCodeAsync_CorrectCode_Confirmed()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1")
				.Build();

			var userId = database.Context.Users.Single().Id;
			database.Reload();

			var identityProvider = GetMockIdentityProvider(GetIdentity("User1"));

			var userService = GetUserService
			(
				database.Context,
				identityProvider.Object
			);

			await userService.SubmitEmailConfirmationCodeAsync("User1EmailConfirmationCode");

			database.Reload();
			var user = database.Context.Users.Single();

			Assert.True(user.EmailAddressConfirmed);
		}

		/// <summary>
		/// Ensures that AddClassroomAdminAsync returns false for an invalid user.
		/// </summary>
		[Fact]
		public async Task AddClassroomAdminAsync_NonExistentUser_ReturnsFalse()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var userService = GetUserService(database.Context);

			var result = await userService.AddClassroomAdminAsync
			(
				"Class1", 
				"NonExistentUser"
			);

			Assert.False(result);
		}

		/// <summary>
		/// Ensures that AddClassroomAdminAsync successfully adds an existing student
		/// in the same class as an admin.
		/// </summary>
		[Fact]
		public async Task AddClassroomAdminAsync_ExistingStudentInSameClass_AddsAdmin()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var userService = GetUserService(database.Context);

			var result = await userService.AddClassroomAdminAsync
			(
				"Class1",
				"User1"
			);

			database.Reload();

			var classroomMemberships = database.Context.ClassroomMemberships
				.Where(cm => cm.User.UserName == "User1")
				.Include(cm => cm.Classroom)
				.ToList();

			Assert.True(result);
			Assert.Single(classroomMemberships);
			Assert.Equal("Class1", classroomMemberships[0].Classroom.Name);
			Assert.Equal(ClassroomRole.Admin, classroomMemberships[0].Role);
		}

		/// <summary>
		/// Ensures that AddClassroomAdminAsync successfully adds an existing admin
		/// in a different class as an admin.
		/// </summary>
		[Fact]
		public async Task AddClassroomAdminAsync_ExistingAdminInDifferentClass_AddsAdmin()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddAdmin("Admin1", "LastName", "FirstName", "Class1", superUser: false)
				.Build();

			var gitHubTeamClient = GetMockGitHubTeamClient("Class2");

			var userService = GetUserService
			(
				database.Context,
				gitHubTeamClient: gitHubTeamClient.Object
			);

			var result = await userService.AddClassroomAdminAsync
			(
				"Class2",
				"Admin1"
			);

			database.Reload();

			var classroomMemberships = database.Context.ClassroomMemberships
				.Where(cm => cm.User.UserName == "Admin1")
				.Include(cm => cm.Classroom)
				.OrderBy(cm => cm.Classroom.Name)
				.ToList();

			Assert.True(result);
			Assert.Equal(2, classroomMemberships.Count);
			Assert.Equal("Class1", classroomMemberships[0].Classroom.Name);
			Assert.Equal(ClassroomRole.Admin, classroomMemberships[0].Role);
			Assert.Equal("Class2", classroomMemberships[1].Classroom.Name);
			Assert.Equal(ClassroomRole.Admin, classroomMemberships[1].Role);
		}

		/// <summary>
		/// Ensures that AddClassroomAdminAsync invites the new admin to the classroom's
		/// GitHub organization.
		/// </summary>
		[Fact]
		public async Task AddClassroomAdminAsync_ExistingAdminInDifferentClass_InvitesToTeam()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddAdmin("Admin1", "LastName", "FirstName", "Class1", 
					false /*superUser*/, "GitHubUser", true /*inGitHubOrg*/)
				.Build();

			var gitHubTeamClient = GetMockGitHubTeamClient("Class2");

			var userService = GetUserService
			(
				database.Context,
				gitHubTeamClient: gitHubTeamClient.Object
			);

			await userService.AddClassroomAdminAsync
			(
				"Class2",
				"Admin1"
			);

			gitHubTeamClient.Verify
			(
				gc => gc.InviteUserToTeamAsync
				(
					"Class2GitHubOrg",
					It.Is<GitHubTeam>(t => t.Name == "LastNameFirstName"),
					"GitHubUser"
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that RemoveClassroomAdminAsync actually removes the admin,
		/// when the admin is not also a student.
		/// </summary>
		[Fact]
		public async Task RemoveClassroomAdminAsync_NotAlsoStudent_RemovesAdmin()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddAdmin("Admin1", "LastName", "FirstName", "Class1",
					false /*superUser*/, "GitHubUser", true /*inGitHubOrg*/)
				.Build();

			var userId = database.Context.Users.Single().Id;
			database.Reload();
			
			var userService = GetUserService(database.Context);

			await userService.RemoveClassroomAdminAsync("Class1", userId);

			database.Reload();

			var classroomMemberships = database.Context.ClassroomMemberships
				.Where(cm => cm.User.UserName == "Admin1")
				.ToList();

			Assert.Empty(classroomMemberships);
		}

		/// <summary>
		/// Ensures that RemoveClassroomAdminAsync actually removes the admin,
		/// when the admin is also a student.
		/// </summary>
		[Fact]
		public async Task RemoveClassroomAdminAsync_AlsoStudent_RemovesAdmin()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var userId = database.Context.Users.Single().Id;
			database.Context.ClassroomMemberships.Single().Role = ClassroomRole.Admin;
			database.Context.SaveChanges();
			database.Reload();

			var userService = GetUserService(database.Context);

			await userService.RemoveClassroomAdminAsync("Class1", userId);

			database.Reload();

			var classroomMemberships = database.Context.ClassroomMemberships
				.Where(cm => cm.User.UserName == "User1")
				.Include(cm => cm.Classroom)
				.OrderBy(cm => cm.Classroom.Name)
				.ToList();

			Assert.Single(classroomMemberships);
			Assert.Equal("Class1", classroomMemberships[0].Classroom.Name);
			Assert.Equal(ClassroomRole.General, classroomMemberships[0].Role);
		}

		/// <summary>
		/// Ensures that AddSectionStudentAsync returns false for an invalid user.
		/// </summary>
		[Fact]
		public async Task AddSectionStudentAsync_NonExistentUser_ReturnsFalse()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			var userService = GetUserService(database.Context);

			var result = await userService.AddSectionStudentAsync
			(
				"Class1",
				"Section1",
				"NonExistentUser"
			);

			Assert.False(result);
		}

		/// <summary>
		/// Ensures that AddSectionStudentAsync successfully adds an existing student
		/// in a different class as a student.
		/// </summary>
		[Fact]
		public async Task AddSectionStudentAsync_ExistingStudentInDifferentClass_AddsAdmin()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var gitHubTeamClient = GetMockGitHubTeamClient("Class2");

			var userService = GetUserService
			(
				database.Context,
				gitHubTeamClient: gitHubTeamClient.Object
			);

			var result = await userService.AddSectionStudentAsync
			(
				"Class2",
				"Section2",
				"User1"
			);

			database.Reload();

			var classroomMemberships = database.Context.ClassroomMemberships
				.Where(cm => cm.User.UserName == "User1")
				.Include(cm => cm.Classroom)
				.OrderBy(cm => cm.Classroom.Name)
				.ToList();

			var sectionMemberships = database.Context.SectionMemberships
				.Where(sm => sm.ClassroomMembership.User.UserName == "User1")
				.Include(sm => sm.Section)
				.OrderBy(sm => sm.Section.Name)
				.ToList();

			Assert.True(result);

			Assert.Equal(2, classroomMemberships.Count);
			Assert.Equal(2, sectionMemberships.Count);

			Assert.Equal("Class1", classroomMemberships[0].Classroom.Name);
			Assert.Equal("Section1", sectionMemberships[0].Section.Name);
			Assert.Equal(ClassroomRole.General, classroomMemberships[0].Role);
			Assert.Equal(SectionRole.Student, sectionMemberships[0].Role);
			Assert.Equal(classroomMemberships[0], sectionMemberships[0].ClassroomMembership);

			Assert.Equal("Class2", classroomMemberships[1].Classroom.Name);
			Assert.Equal("Section2", sectionMemberships[1].Section.Name);
			Assert.Equal(ClassroomRole.General, classroomMemberships[1].Role);
			Assert.Equal(SectionRole.Student, sectionMemberships[1].Role);
			Assert.Equal(classroomMemberships[1], sectionMemberships[1].ClassroomMembership);
		}

		/// <summary>
		/// Ensures that AddClassroomAsync invites the new admin to the classroom's
		/// GitHub organization.
		/// </summary>
		[Fact]
		public async Task AddSectionStudentAsync_ExistingStudentInDifferentClass_InvitesToTeam()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Section1")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var gitHubTeamClient = GetMockGitHubTeamClient("Class2");

			var userService = GetUserService
			(
				database.Context,
				gitHubTeamClient: gitHubTeamClient.Object
			);

			var result = await userService.AddSectionStudentAsync
			(
				"Class2",
				"Section2",
				"User1"
			);

			gitHubTeamClient.Verify
			(
				gc => gc.InviteUserToTeamAsync
				(
					"Class2GitHubOrg",
					It.Is<GitHubTeam>(t => t.Name == "LastNameFirstName"),
					"GitHubUser"
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures that AddClassroomAsync successfully adds an existing admin
		/// in the same class as a student.
		/// </summary>
		[Fact]
		public async Task AddSectionStudentAsync_ExistingAdminInSameClass_AddsStudent()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddAdmin("Admin1", "LastName", "FirstName", "Class1", superUser: false)
				.Build();

			var gitHubTeamClient = GetMockGitHubTeamClient("Class1");

			var userService = GetUserService
			(
				database.Context,
				gitHubTeamClient: gitHubTeamClient.Object
			);

			var result = await userService.AddSectionStudentAsync
			(
				"Class1",
				"Section1",
				"Admin1"
			);

			database.Reload();

			var classroomMemberships = database.Context.ClassroomMemberships
				.Where(cm => cm.User.UserName == "Admin1")
				.Include(cm => cm.Classroom)
				.OrderBy(cm => cm.Classroom.Name)
				.ToList();

			var sectionMemberships = database.Context.SectionMemberships
				.Where(sm => sm.ClassroomMembership.User.UserName == "Admin1")
				.Include(sm => sm.Section)
				.OrderBy(sm => sm.Section.Name)
				.ToList();

			Assert.True(result);

			Assert.Single(classroomMemberships);
			Assert.Single(sectionMemberships);

			Assert.Equal("Class1", classroomMemberships[0].Classroom.Name);
			Assert.Equal("Section1", sectionMemberships[0].Section.Name);
			Assert.Equal(ClassroomRole.Admin, classroomMemberships[0].Role);
			Assert.Equal(SectionRole.Student, sectionMemberships[0].Role);
			Assert.Equal(classroomMemberships[0], sectionMemberships[0].ClassroomMembership);
		}

		/// <summary>
		/// Ensures that RemoveSectionStudentAsync actually removes the student,
		/// when the student is not also an admin.
		/// </summary>
		[Fact]
		public async Task RemoveSectionStudentAsync_RemovesStudent()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.Build();

			var userId = database.Context.Users.Single().Id;
			database.Reload();

			var userService = GetUserService(database.Context);

			await userService.RemoveSectionStudentAsync("Class1", "Section1", userId);

			database.Reload();

			var sectionMemberships = database.Context.SectionMemberships
				.Where(sm => sm.ClassroomMembership.User.UserName == "User1")
				.ToList();

			Assert.Empty(sectionMemberships);
		}

		/// <summary>
		/// Returns a mock identity.
		/// </summary>
		private Mock<IIdentityProvider> GetMockIdentityProvider(Model.Users.Identity identity)
		{
			var identityProvider = new Mock<IIdentityProvider>();
			identityProvider
				.Setup(ip => ip.CurrentIdentity)
				.Returns(identity);

			return identityProvider;
		}

		/// <summary>
		/// Returns a mock GitHub user Client.
		/// </summary>
		private static Mock<IGitHubUserClient> GetMockGitHubUserClient(bool userExists)
		{
			var gitHubUserClient = new Mock<IGitHubUserClient>();
			gitHubUserClient
				.Setup(gc => gc.DoesUserExistAsync("GitHubUser"))
				.ReturnsAsync(userExists);

			return gitHubUserClient;
		}

		/// <summary>
		/// Returns a mock GitHub organization client.
		/// </summary>
		private static Mock<IGitHubOrganizationClient> GetMockGitHubOrgClient(bool userInOrg)
		{
			var gitHubOrgClient = new Mock<IGitHubOrganizationClient>();
			gitHubOrgClient
				.Setup(gc => gc.CheckMemberAsync("Class1GitHubOrg", "GitHubUser"))
				.ReturnsAsync(userInOrg);

			return gitHubOrgClient;
		}

		/// <summary>
		/// Returns a mock GitHub team client.
		/// </summary>
		private static Mock<IGitHubTeamClient> GetMockGitHubTeamClient(string classroomName)
		{
			var team = new GitHubTeam(1, "LastNameFirstName");

			var gitHubTeamClient = new Mock<IGitHubTeamClient>();
			gitHubTeamClient
				.Setup(gc => gc.CreateTeamAsync($"{classroomName}GitHubOrg", "LastNameFirstName"))
				.ReturnsAsync(team);
			gitHubTeamClient
				.Setup(gc => gc.GetTeamAsync($"{classroomName}GitHubOrg", "LastNameFirstName"))
				.ReturnsAsync(team);
			gitHubTeamClient
				.Setup(gc => gc.InviteUserToTeamAsync($"{classroomName}GitHubOrg", team, "GitHubUser"))
				.Returns(Task.CompletedTask);
			gitHubTeamClient
				.Setup(gc => gc.RemoveUserFromTeamAsync($"{classroomName}GitHubOrg", team, "OldGitHubUser"))
				.Returns(Task.CompletedTask);

			return gitHubTeamClient;
		}

		/// <summary>
		/// Returns a mock e-mail provider.
		/// </summary>
		private static Mock<IEmailProvider> GetMockEmailProvider(string emailAddress)
		{
			var emailProvider = new Mock<IEmailProvider>();
			emailProvider
				.Setup
				(
					ep => ep.SendMessageAsync
					(
						It.Is<IList<EmailRecipient>>
						(
							to => to.Count == 1 && to[0].EmailAddress == emailAddress
						),
						It.IsAny<string>(),
						It.IsAny<string>()
					)
				).Returns(Task.CompletedTask);

			return emailProvider;
		}

		/// <summary>
		/// Returns a user service.
		/// </summary>
		private IUserService GetUserService(
			DatabaseContext dbContext = null,
			IIdentityProvider identityProvider = null,
			IGitHubUserClient gitHubUserClient = null,
			IGitHubOrganizationClient gitHubOrgClient = null,
			IGitHubTeamClient gitHubTeamClient = null,
			IEmailProvider emailProvider = null,
			ActivationToken activationToken = null)
		{
			return new UserService
			(
				dbContext,
				identityProvider,
				gitHubUserClient,
				gitHubOrgClient,
				gitHubTeamClient,
				emailProvider,
				activationToken
			);
		}

		/// <summary>
		/// Returns an identity.
		/// </summary>
		private Model.Users.Identity GetIdentity(string userName, string uniqueId = null)
		{
			return new Model.Users.Identity
			(
				uniqueId ?? $"{userName}Id",
				userName,
				"FirstName",
				"LastName"
			);
		}
	}
}
