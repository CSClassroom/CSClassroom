using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.Email
{
	/// <summary>
	/// An e-mail address.
	/// </summary>
	public class EmailRecipient
	{
		/// <summary>
		/// The recipient's name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The recipient's e-mail address.
		/// </summary>
		public string EmailAddress { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public EmailRecipient(string name, string emailAddress)
		{
			Name = name;
			EmailAddress = emailAddress;
		}
	}

	/// <summary>
	/// Sends mail messages.
	/// </summary>
	public interface IEmailProvider
	{
		/// <summary>
		/// Send a mail message.
		/// </summary>
		Task SendMessageAsync(
			IList<EmailRecipient> recipients,
			string subject,
			string body);
	}
}
