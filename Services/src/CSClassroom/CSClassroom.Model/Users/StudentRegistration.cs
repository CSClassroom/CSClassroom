using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// A registration for a new student in a classroom.
	/// </summary>
	public class StudentRegistration
	{
		/// <summary>
		/// The first name of the user.
		/// </summary>
		[Required]
		[Display
		(
			Name = "First Name",
			Description = "Enter the first name you go by."
		)]
		public string FirstName { get; set; }

		/// <summary>
		/// The contact e-mail address for the user.
		/// </summary>
		[Required]
		[EmailAddress]
		[Display
		(
			Name = "Email Address",
			Description = "Enter an e-mail address you regularly check. "
				+ "You will be receiving e-mails you need to take action on, including project feedback."
		)]
		public string EmailAddress { get; set; }

		/// <summary>
		/// The GitHub login for the user.
		/// </summary>
		[Required]
		[Display
		(
			Name = "GitHub Login",
			Description = "Enter your GitHub username (NOT your password, or e-mail address)."
		)]
		public string GitHubLogin { get; set; }
	}
}
