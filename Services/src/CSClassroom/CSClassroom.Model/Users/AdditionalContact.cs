using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// An additional contact for a student.
	/// </summary>
	public class AdditionalContact
	{
		/// <summary>
		/// The unique ID for the additional contact.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// The user.
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// The last name of the contact.
		/// </summary>
		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		/// <summary>
		/// The first name of the contact.
		/// </summary>
		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		/// <summary>
		/// The e-mail address for the additional contact.
		/// </summary>
		[Required]
		[Display(Name = "E-mail Address")]
		public string EmailAddress { get; set; }
	}
}
