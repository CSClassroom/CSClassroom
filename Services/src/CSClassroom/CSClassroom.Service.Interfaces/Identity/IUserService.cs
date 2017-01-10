using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Identity
{
	/// <summary>
	/// The result for registering a new user.
	/// </summary>
	public enum RegisterNewUserResult
	{
		Success,
		SectionNotFound,
		SectionNotOpen,
		AlreadyRegistered,
		Failed
	}

	/// <summary>
	/// The result for registering a new user.
	/// </summary>
	public enum RegisterExistingUserResult
	{
		Success,
		SectionNotFound,
		SectionNotOpen,
		AlreadyRegistered
	}

	/// <summary>
	/// Manages users on the service.
	/// </summary>
	public interface IUserService
	{
		/// <summary>
		/// Returns the currently signed-in user, updating the user's
		/// information if necessary.
		/// </summary>
		Task<User> GetAndUpdateCurrentUserAsync();

		/// <summary>
		/// Returns the user with the given ID.
		/// </summary>
		Task<User> GetUserAsync(int userId);

		/// <summary>
		/// Updates the given user.
		/// </summary>
		Task<bool> UpdateUserAsync(
			User user,
			string confirmationUrlBuilder,
			IModelErrorCollection modelErrors);

		/// <summary>
		/// Returns whether or not any users are registered.
		/// </summary>
		Task<bool> AnyRegisteredUsersAsync();

		/// <summary>
		/// Called to register the first super-user.
		/// </summary>
		Task<RegisterNewUserResult> RegisterFirstSuperUserAsync(
			SuperUserRegistration registration,
			IModelErrorCollection errors);

		/// <summary>
		/// Registers a new user.
		/// </summary>
		Task<RegisterNewUserResult> RegisterNewStudentAsync(
			string classroomName,
			string sectionName,
			StudentRegistration registration,
			string confirmationUrl,
			IModelErrorCollection errors);

		/// <summary>
		/// Registers an existing user for a new classroom.
		/// </summary>
		Task<RegisterExistingUserResult> RegisterExistingStudentAsync(
			string classroomName,
			string sectionName);

		/// <summary>
		/// Returns whether or not the current user can view or edit 
		/// information about the given user.
		/// </summary>
		Task<bool> CanViewAndEditUserAsync(int userId);

		/// <summary>
		/// Sends a confirmation e-mail with a link that will mark the user as confirmed.
		/// </summary>
		Task ResendEmailConfirmationAsync(int userId, string confirmationUrlBuilder);

		/// <summary>
		/// Submits the user's e-mail confirmation code.
		/// </summary>
		Task SubmitEmailConfirmationCodeAsync(string emailConfirmationCode);

		/// <summary>
		/// Adds a classroom admin. Returns false if the user doesn't exist.
		/// </summary>
		Task<bool> AddClassroomAdminAsync(
			string classroomName, 
			string userName);

		/// <summary>
		/// Removes a classroom admin. 
		/// </summary>
		Task RemoveClassroomAdminAsync(
			string classroomName,
			int userId);

		/// <summary>
		/// Adds a student to a section. Returns false if the user doesn't exist.
		/// </summary>
		Task<bool> AddSectionStudentAsync(
			string classroomName,
			string sectionName,
			string userName);

		/// <summary>
		/// Removes a student from a section.
		/// </summary>
		Task RemoveSectionStudentAsync(
			string classroomName,
			string sectionName,
			int userId);
	}
}
