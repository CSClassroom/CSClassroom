using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.Email
{
	/// <summary>
	/// Does not send any e-mail messages.
	/// </summary>
	public class DisabledEmailProvider : IEmailProvider
	{
		/// <summary>
		/// The default e-mail address to send e-mails from, unless overridden.
		/// </summary>
		public string DefaultFromAddress
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Send a mail message.
		/// </summary>
		public Task SendMessageAsync(
			IList<EmailRecipient> recipients,
			string subject,
			string body,
			bool broadcast,
			EmailSender customSender = null,
			ThreadInfo threadInfo = null)
		{
			// Do nothing.
			return Task.FromResult<bool>(false);
		}
	}
}
