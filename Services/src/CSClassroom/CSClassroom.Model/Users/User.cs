using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// A user.
	/// </summary>
	public class User
	{
		/// <summary>
		/// The unique ID for the user.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The unique ID for the user.
		/// </summary>
		public string UniqueId { get; set; }

		/// <summary>
		/// The username for the user.
		/// </summary>
		[Display(Name = "Username")]
		public string UserName { get; set; }

		/// <summary>
		/// The first name of the user.
		/// </summary>
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		/// <summary>
		/// The last name of the user.
		/// </summary>
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		/// <summary>
		/// The name shown to the public when posting.
		/// </summary>
		[Display(Name = "Publicly Displayed Name")]
		public string PublicName { get; set; }

		/// <summary>
		/// The contact e-mail address for the user.
		/// </summary>
		[Display(Name = "E-mail Address")]
		public string EmailAddress { get; set; }

		/// <summary>
		/// The confirmation code required to confirm the e-mail address.
		/// </summary>
		public string EmailConfirmationCode { get; set; }

		/// <summary>
		/// Whether or not the e-mail address was confirmed.
		/// </summary>
		public bool EmailAddressConfirmed { get; set; }

		/// <summary>
		/// The user's login for GitHub.
		/// </summary>
		[Display(Name = "GitHub Login")]
		public string GitHubLogin { get; set; }

		/// <summary>
		/// Can perform all operations on the site.
		/// </summary>
		public bool SuperUser { get; set; }

		/// <summary>
		/// The classroom memberships for this user.
		/// </summary>
		public virtual IList<ClassroomMembership> ClassroomMemberships { get; set; }

		/// <summary>
		/// Builds for the user.
		/// </summary>
		public virtual IList<Commit> Commits { get; set; }

		/// <summary>
		/// Additional contacts for the user.
		/// </summary>
		[Display(Name = "Additional Contacts")]
		public virtual IList<AdditionalContact> AdditionalContacts { get; set; }

		/// <summary>
		/// Returns whether or not the user is activated.
		/// </summary>
		public bool IsActivated =>
			   EmailAddressConfirmed
			&& (ClassroomMemberships?.All(m => m.InGitHubOrganization) ?? true);

		/// <summary>
		/// Returns the user's publicly displayed name if present,
		/// or the user's first and last name if not.
		/// </summary>
		public string PubliclyDisplayedName => PublicName ?? $"{FirstName} {LastName}";
	}
}
