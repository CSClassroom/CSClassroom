using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// A registration for the initial super-user for the service.
	/// </summary>
	public class SuperUserRegistration
	{
		/// <summary>
		/// The activation token.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Activation Token",
			Description = "Enter the token set during the initial deployment."
		)]
		public string ActivationToken { get; set; }

		/// <summary>
		/// The first name of the user.
		/// </summary>
		[Required]
		[Display
		(
			Name = "First Name",
			Description = "Enter your first name."
		)]
		public string FirstName { get; set; }

		/// <summary>
		/// The last name of the user.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Last Name",
			Description = "Enter your last name."
		)]
		public string LastName { get; set; }

		/// <summary>
		/// The contact e-mail address for the user.
		/// </summary>
		[Required]
		[EmailAddress]
		[Display
		(
			Name = "Email Address",
			Description = "Enter your e-mail address."
		)]
		public string EmailAddress { get; set; }

		/// <summary>
		/// The GitHub login for the user.
		/// </summary>
		[Required]
		[Display
		(
			Name = "GitHub Login",
			Description = "Enter your GitHub username."
		)]
		public string GitHubLogin { get; set; }
	}
}
