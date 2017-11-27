using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Identity
{
	/// <summary>
	/// Manages users on the service.
	/// </summary>
	public class UserService : IUserService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The identity provider.
		/// </summary>
		private readonly IIdentityProvider _identityProvider;

		/// <summary>
		/// The GitHub user client.
		/// </summary>
		private readonly IGitHubUserClient _gitHubUserClient;

		/// <summary>
		/// The GitHub organization client.
		/// </summary>
		private readonly IGitHubOrganizationClient _gitHubOrgClient;

		/// <summary>
		/// The GitHub team client.
		/// </summary>
		private readonly IGitHubTeamClient _gitHubTeamClient;

		/// <summary>
		/// The e-mail provider.
		/// </summary>
		private readonly IEmailProvider _emailProvider;

		/// <summary>
		/// The token required to activate the service.
		/// </summary>
		private readonly ActivationToken _activationToken;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserService(
			DatabaseContext dbContext,
			IIdentityProvider identityProvider,
			IGitHubUserClient gitHubUserClient, 
			IGitHubOrganizationClient gitHubOrgClient,
			IGitHubTeamClient gitHubTeamClient,
			IEmailProvider emailProvider,
			ActivationToken activationToken)
		{
			_dbContext = dbContext;
			_identityProvider = identityProvider;
			_gitHubUserClient = gitHubUserClient;
			_gitHubOrgClient = gitHubOrgClient;
			_gitHubTeamClient = gitHubTeamClient;
			_emailProvider = emailProvider;
			_activationToken = activationToken;
		}

		/// <summary>
		/// Returns the currently signed-in user, updating the user's
		/// information if necessary.
		/// </summary>
		public async Task<User> GetAndUpdateCurrentUserAsync()
		{
			var identity = _identityProvider.CurrentIdentity;
			if (identity == null)
				return null;

			var user = await _dbContext.Users
				.SingleOrDefaultAsync(u => u.UniqueId == identity.UniqueId);

			if (user != null)
			{
				await LoadUserAsync(user);

				// Update the username if it changed
				if (user.UserName != _identityProvider.CurrentIdentity.UserName)
				{
					user.UserName = _identityProvider.CurrentIdentity.UserName;
					await _dbContext.SaveChangesAsync();
				}
			}

			return user;
		}

		/// <summary>
		/// Returns the user with the given ID.
		/// </summary>
		public async Task<User> GetUserAsync(int userId)
		{
			var user = await _dbContext.Users
				.Include(u => u.AdditionalContacts)
				.SingleAsync(u => u.Id == userId);

			await LoadUserAsync(user);

			return user;
		}
		
		/// <summary>
		/// Updates the given user.
		/// </summary>
		public async Task<bool> UpdateUserAsync(
			User user,
			string confirmationUrlBuilder,
			IModelErrorCollection modelErrors)
		{
			var existingUser = await _dbContext.Users
				.Include(u => u.ClassroomMemberships)
					.ThenInclude(cm => cm.Classroom)
				.Include(u => u.AdditionalContacts)
				.SingleAsync(u => u.Id == user.Id);

			bool updatedEmail = false;

			if (user.GitHubLogin != existingUser.GitHubLogin)
			{
				if (!await _gitHubUserClient.DoesUserExistAsync(user.GitHubLogin))
				{
					modelErrors.AddError("GitHubLogin", "The GitHub username does not exist.");
					return false;
				}

				if (existingUser.ClassroomMemberships != null)
				{
					foreach (var membership in existingUser.ClassroomMemberships)
					{
						var orgName = membership.Classroom.GitHubOrganization;

						var team = await _gitHubTeamClient.GetTeamAsync
						(
							orgName,
							membership.GitHubTeam
						);

						await _gitHubTeamClient.InviteUserToTeamAsync
						(
							orgName,
							team,
							user.GitHubLogin
						);

						await _gitHubTeamClient.RemoveUserFromTeamAsync
						(
							orgName,
							team,
							existingUser.GitHubLogin
						);

						membership.InGitHubOrganization = false;
					}
				}

				existingUser.GitHubLogin = user.GitHubLogin;
			}

			if (user.PublicName != existingUser.PublicName)
			{
				existingUser.PublicName = user.PublicName;
			}

			if (user.EmailAddress != existingUser.EmailAddress)
			{
				existingUser.EmailAddress = user.EmailAddress;
				existingUser.EmailAddressConfirmed = false;
				existingUser.EmailConfirmationCode = GenerateEmailConfirmationCode();

				updatedEmail = true;
			}

			foreach (var additionalContact in existingUser.AdditionalContacts.ToList())
			{
				var modifiedContact = user.AdditionalContacts
					?.SingleOrDefault(ac => ac.Id == additionalContact.Id);
				if (modifiedContact != null)
				{
					additionalContact.LastName = modifiedContact.LastName;
					additionalContact.FirstName = modifiedContact.FirstName;
					additionalContact.EmailAddress = modifiedContact.EmailAddress;
				}
				else
				{
					existingUser.AdditionalContacts.Remove(additionalContact);
				}
			}

			if (user.AdditionalContacts != null)
			{
				foreach (var potentialNewContact in user.AdditionalContacts)
				{
					if (!existingUser.AdditionalContacts.Any(ac => ac.Id == potentialNewContact.Id))
					{
						existingUser.AdditionalContacts.Add(potentialNewContact);
					}
				}
			}

			await _dbContext.SaveChangesAsync();

			if (updatedEmail)
			{
				await SendUserInvitationMailAsync(existingUser, confirmationUrlBuilder);
			}

			return true;
		}

		/// <summary>
		/// Loads the given user.
		/// </summary>
		private async Task LoadUserAsync(User user)
		{
			if (user.ClassroomMemberships != null)
				return;

			// Work around the fact that EF7 doesn't yet support nested include expressions

			await _dbContext.Entry(user)
				.Collection(u => u.ClassroomMemberships)
				.Query()
				.Include(cm => cm.Classroom)
				.Include(cm => cm.SectionMemberships)
					.ThenInclude(sm => sm.Section)
				.LoadAsync();

			await EnsureGitHubMembershipsUpdated(user);
		}

		/// <summary>
		/// Returns whether or not any users are registered.
		/// </summary>
		public async Task<bool> AnyRegisteredUsersAsync()
		{
			return await _dbContext.Users.AnyAsync();
		}

		/// <summary>
		/// Called to register the first super-user.
		/// </summary>
		public async Task<RegisterNewUserResult> RegisterFirstSuperUserAsync(
			SuperUserRegistration registration,
			IModelErrorCollection errors)
		{
			if (await AnyRegisteredUsersAsync())
			{
				return RegisterNewUserResult.AlreadyRegistered;
			}

			if (registration.ActivationToken != _activationToken.Value)
			{
				errors.AddError("ActivationToken", "Incorrect activation token.");
				return RegisterNewUserResult.Failed;
			}

			if (!await _gitHubUserClient.DoesUserExistAsync(registration.GitHubLogin))
			{
				errors.AddError("GitHubLogin", "The GitHub username does not exist.");
				return RegisterNewUserResult.Failed;
			}

			User user = new User()
			{
				UniqueId = _identityProvider.CurrentIdentity.UniqueId,
				UserName = _identityProvider.CurrentIdentity.UserName,
				FirstName = registration.FirstName,
				LastName = registration.LastName,
				EmailAddress = registration.EmailAddress,
				EmailAddressConfirmed = true,
				GitHubLogin = registration.GitHubLogin,
				SuperUser = true
			};
			
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			return RegisterNewUserResult.Success;
		}

		/// <summary>
		/// Registers a user.
		/// </summary>
		public async Task<RegisterNewUserResult> RegisterNewStudentAsync(
			string classroomName,
			string sectionName,
			StudentRegistration registration,
			string confirmationUrlBuilder,
			IModelErrorCollection errors)
		{
			var section = _dbContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.Include(s => s.Classroom)
				.SingleOrDefault(s => s.Name == sectionName);

			if (section == null)
			{
				return RegisterNewUserResult.SectionNotFound;
			}

			if (!section.AllowNewRegistrations)
			{
				return RegisterNewUserResult.SectionNotOpen;
			}

			var user = await GetAndUpdateCurrentUserAsync();
			if (user != null)
			{
				return RegisterNewUserResult.AlreadyRegistered;
			}
			
			if (!await _gitHubUserClient.DoesUserExistAsync(registration.GitHubLogin))
			{
				errors.AddError("GitHubLogin", "The GitHub username does not exist.");
				return RegisterNewUserResult.Failed;
			}

			user = new User()
			{
				UniqueId = _identityProvider.CurrentIdentity.UniqueId,
				UserName = _identityProvider.CurrentIdentity.UserName,
				FirstName = registration.FirstName,
				LastName = _identityProvider.CurrentIdentity.LastName,
				EmailAddress = registration.EmailAddress,
				EmailConfirmationCode = GenerateEmailConfirmationCode(),
				EmailAddressConfirmed = false,
				GitHubLogin = registration.GitHubLogin,
				SuperUser = false
			};

			var membership = await EnsureSectionMembershipAsync(user, section, SectionRole.Student);		
			await EnsureUserInGithubOrgAsync(user, membership.ClassroomMembership);
			await SendUserInvitationMailAsync(user, confirmationUrlBuilder);

			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			return RegisterNewUserResult.Success;
		}

		/// <summary>
		/// Registers an existing user for a new classroom.
		/// </summary>
		public async Task<RegisterExistingUserResult> RegisterExistingStudentAsync(
			string classroomName,
			string sectionName)
		{
			var section = _dbContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.Include(s => s.Classroom)
				.SingleOrDefault(s => s.Name == sectionName);

			if (section == null)
			{
				return RegisterExistingUserResult.SectionNotFound;
			}

			if (!section.AllowNewRegistrations)
			{
				return RegisterExistingUserResult.SectionNotOpen;
			}

			var user = await GetAndUpdateCurrentUserAsync();
			var sectionMembership = await EnsureSectionMembershipAsync(
				user, 
				section, 
				SectionRole.Student);

			if (_dbContext.Entry(sectionMembership).State == EntityState.Unchanged)
			{
				return RegisterExistingUserResult.AlreadyRegistered;
			}

			await _dbContext.SaveChangesAsync();
			await EnsureUserInGithubOrgAsync(user, sectionMembership.ClassroomMembership);

			return RegisterExistingUserResult.Success;
		}

		/// <summary>
		/// Adds a classroom admin. Returns false if the user doesn't exist.
		/// </summary>
		public async Task<bool> AddClassroomAdminAsync(
			string classroomName,
			string userName)
		{
			var user = _dbContext.Users.SingleOrDefault(u => u.UserName == userName);
			if (user == null)
			{
				return false;
			}

			await LoadUserAsync(user);

			var classroom = await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.SingleAsync();

			var classroomMembership = await EnsureClassroomMembershipAsync
			(
				user, 
				classroom, 
				ClassroomRole.Admin
			);

			await _dbContext.SaveChangesAsync();
			await EnsureUserInGithubOrgAsync(user, classroomMembership);

			return true;
		}

		/// <summary>
		/// Removes a classroom admin.
		/// </summary>
		public async Task RemoveClassroomAdminAsync(
			string classroomName,
			int userId)
		{
			var classroom = await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.SingleAsync();

			var membership = await _dbContext.ClassroomMemberships
				.Where(m => m.ClassroomId == classroom.Id && m.UserId == userId)
				.Include(m => m.SectionMemberships)
				.SingleAsync();

			if (membership.SectionMemberships.Any())
			{
				membership.Role = ClassroomRole.General;
			}
			else
			{
				_dbContext.ClassroomMemberships.Remove(membership);
			}

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Adds a student to a section. Returns false if the user doesn't exist.
		/// </summary>
		public async Task<bool> AddSectionStudentAsync(
			string classroomName,
			string sectionName,
			string userName)
		{
			var user = await _dbContext.Users
				.SingleOrDefaultAsync(u => u.UserName == userName);

			if (user == null)
			{
				return false;
			}

			await LoadUserAsync(user);

			var section = await _dbContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.Include(s => s.Classroom)
				.SingleAsync(s => s.Name == sectionName);

			var sectionMembership = await EnsureSectionMembershipAsync(user, section, SectionRole.Student);

			await _dbContext.SaveChangesAsync();
			await EnsureUserInGithubOrgAsync(user, sectionMembership.ClassroomMembership);

			return true;
		}

		/// <summary>
		/// Removes a student from a section.
		/// </summary>
		public async Task RemoveSectionStudentAsync(
			string classroomName,
			string sectionName,
			int userId)
		{
			var section = await _dbContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.Include(s => s.Classroom)
				.SingleAsync(s => s.Name == sectionName);

			var membership = await _dbContext.SectionMemberships
				.Where(m => m.SectionId == section.Id && m.ClassroomMembership.UserId == userId)
				.SingleAsync();

			_dbContext.SectionMemberships.Remove(membership);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Sends a confirmation e-mail with a link that will mark the user as confirmed.
		/// </summary>
		public async Task ResendEmailConfirmationAsync(int userId, string confirmationUrlBuilder)
		{
			var user = await GetUserAsync(userId);

			await SendUserInvitationMailAsync(user, confirmationUrlBuilder);
		}

		/// <summary>
		/// Attempts to confirm an e-mail address. Returns whether the address was confirmed.
		/// </summary>
		public async Task SubmitEmailConfirmationCodeAsync(string emailConfirmationCode)
		{
			var currentUser = await GetAndUpdateCurrentUserAsync();

			if (emailConfirmationCode == currentUser.EmailConfirmationCode)
			{
				currentUser.EmailAddressConfirmed = true;
				_dbContext.Update(currentUser);
				await _dbContext.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Ensures a classroom membership exists for the given user and role.
		/// The caller is responsible for saving changes.
		/// </summary>
		private async Task<ClassroomMembership> EnsureClassroomMembershipAsync(
			User user,
			Classroom classroom,
			ClassroomRole role)
		{
			var classroomMembership = user?.ClassroomMemberships
				?.SingleOrDefault(m => m.ClassroomId == classroom.Id);

			if (user.ClassroomMemberships == null)
			{
				user.ClassroomMemberships = new List<ClassroomMembership>();
			}

			if (classroomMembership == null)
			{
				classroomMembership = new ClassroomMembership()
				{
					ClassroomId = classroom.Id,
					Classroom = classroom,
					InGitHubOrganization = false,
					GitHubTeam = await GetNewGitHubTeamNameAsync(classroom, user),
					Role = role
				};

				user.ClassroomMemberships.Add(classroomMembership);
			}
			else if (role > classroomMembership.Role)
			{
				classroomMembership.Role = role;
			}

			return classroomMembership;
		}

		/// <summary>
		/// Ensures a section membership exists for the given user and role.
		/// The caller is responsible for saving changes.
		/// </summary>
		private async Task<SectionMembership> EnsureSectionMembershipAsync(
			User user,
			Section section,
			SectionRole role)
		{
			var classroomMembership = await EnsureClassroomMembershipAsync(
				user,
				section.Classroom,
				ClassroomRole.General);

			if (classroomMembership.SectionMemberships == null)
			{
				classroomMembership.SectionMemberships = new List<SectionMembership>();
			}

			var sectionMembership = classroomMembership.SectionMemberships
				.SingleOrDefault(m => m.SectionId == section.Id);

			if (sectionMembership == null)
			{
				sectionMembership = new SectionMembership()
				{
					ClassroomMembershipId = classroomMembership.Id,
					ClassroomMembership = classroomMembership,
					SectionId = section.Id,
					Section = section,
					Role = role
				};

				classroomMembership.SectionMemberships.Add(sectionMembership);
			}
			else if (role > sectionMembership.Role)
			{
				sectionMembership.Role = role;
			}

			return sectionMembership;
		}

		/// <summary>
		/// Sends the user an invitation mail.
		/// </summary>
		private async Task SendUserInvitationMailAsync(User user, string confirmUrlBuilder)
		{
			var confirmUrl = confirmUrlBuilder.Replace("REPLACE", user.EmailConfirmationCode);

			await _emailProvider.SendMessageAsync
			(
				new List<EmailRecipient>()
				{
					new EmailRecipient
					(
						$"{user.FirstName} {user.LastName}",
						user.EmailAddress
					)
				},
				"Confirm your e-mail address with CS Classroom",
				$"Hi {user.FirstName},<br><br>Thank you for registering for CS Classroom. Please <a href=\"{confirmUrl}\">click here</a> to confirm your e-mail address."
			);
		}

		/// <summary>
		/// Invites the user to a new GitHub team.
		/// </summary>
		private async Task EnsureUserInGithubOrgAsync(User user, ClassroomMembership membership)
		{
			if (membership.InGitHubOrganization)
				return;

			var team = await _gitHubTeamClient.CreateTeamAsync
			(
				membership.Classroom.GitHubOrganization, 
				membership.GitHubTeam
			);

			await _gitHubTeamClient.InviteUserToTeamAsync
			(
				membership.Classroom.GitHubOrganization,
				team,
				user.GitHubLogin
			);
		}

		/// <summary>
		/// Returns whether or not the current user can view or edit 
		/// information about the given user.
		/// </summary>
		public async Task<bool> CanViewAndEditUserAsync(int userId)
		{
			var currentUser = await GetAndUpdateCurrentUserAsync();

			if (currentUser.Id == userId || currentUser.SuperUser)
				return true;

			var userToView = await GetUserAsync(userId);

			var currentUserAdminClassrooms = currentUser.ClassroomMemberships
				.Where(m => m.Role >= ClassroomRole.Admin)
				.Select(cm => cm.Classroom)
				.ToList();

			var userToViewClassrooms = userToView.ClassroomMemberships
				.Select(cm => cm.Classroom)
				.ToList();

			if (currentUserAdminClassrooms.Intersect(userToViewClassrooms).Any())
				return true;

			return false;
		}

		/// <summary>
		/// Updates the status of all GitHub memberships for the user.
		/// </summary>
		private async Task EnsureGitHubMembershipsUpdated(User user)
		{
			if (user.GitHubLogin == null || user.ClassroomMemberships == null)
				return;

			bool statusUpdated = false;
			foreach (var membership in user.ClassroomMemberships)
			{
				statusUpdated = await EnsureGitHubMembershipUpdated(user, membership) 
					|| statusUpdated;
			}

			if (statusUpdated)
			{
				await _dbContext.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Returns whether or not the given user is in the classroom's GitHub organization.
		/// </summary>
		private async Task<bool> EnsureGitHubMembershipUpdated(
			User user, 
			ClassroomMembership membership)
		{
			if (membership.InGitHubOrganization)
			{
				return true;
			}
			
			await _dbContext.Entry(membership)
				.Reference(cm => cm.Classroom)
				.LoadAsync();

			var memberInOrganization = await _gitHubOrgClient.CheckMemberAsync
			(
				membership.Classroom.GitHubOrganization,
				user.GitHubLogin
			);
			
			if (memberInOrganization)
			{
				membership.InGitHubOrganization = true;
				_dbContext.ClassroomMemberships.Update(membership);

				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Returns a GitHub repository suffix that is unique class-wide.
		/// </summary>
		private async Task<string> GetNewGitHubTeamNameAsync(Classroom classroom, User user)
		{
			var proposedSuffix = new string
			(
				$"{user.LastName}{user.FirstName}"
					.Where(c => char.IsLetter(c))
					.ToArray()
			);

			var membersWithSuffix = await _dbContext.ClassroomMemberships
				.Where(classMember => classMember.ClassroomId == classroom.Id)
				.Where(classMember => classMember.GitHubTeam.StartsWith(proposedSuffix))
				.ToListAsync();

			if (membersWithSuffix.Count == 0)
				return proposedSuffix;

			const int maxMembersWithSuffix= 100;
			for (int i = 2; i < maxMembersWithSuffix; i++)
			{
				var newProposedSuffix = $"{proposedSuffix}{i}";
				var existingUser = membersWithSuffix.SingleOrDefault
				(
					classMember => classMember.GitHubTeam == newProposedSuffix
				);

				if (existingUser == null)
				{
					return newProposedSuffix;
				}
			}

			throw new InvalidOperationException("Unable to find GitHub repository suffix for user.");
		}
		
		/// <summary>
		/// Generates an e-mail confirmation code.
		/// </summary>
		private static string GenerateEmailConfirmationCode()
		{
			return new string
			(
				Guid.NewGuid()
					.ToString()
					.Where(char.IsLetterOrDigit)
					.ToArray()
			);
		}
	}
}
