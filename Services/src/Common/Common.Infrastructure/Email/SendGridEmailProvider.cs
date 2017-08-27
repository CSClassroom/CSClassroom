using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CSC.Common.Infrastructure.Email
{
	/// <summary>
	/// Sends mail messages.
	/// </summary>
	public class SendGridEmailProvider : IEmailProvider
	{
		/// <summary>
		/// The SendGrid API key.
		/// </summary>
		private readonly string _apiKey;

		/// <summary>
		/// The from address for all messages.
		/// </summary>
		private readonly string _fromAddress;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SendGridEmailProvider(string apiKey, string fromAddress)
		{
			_apiKey = apiKey;
			_fromAddress = fromAddress;
		}

		/// <summary>
		/// Send a mail message.
		/// </summary>
		public async Task SendMessageAsync(
			IList<EmailRecipient> recipients,
			string subject, 
			string body)
		{
			if (_apiKey == null)
			{
				// E-mail is disabled.
				return;
			}

			var from = new EmailAddress(_fromAddress, "CS Classroom");
			var tos = recipients.Select(r => new EmailAddress(r.EmailAddress, r.Name)).ToList();
			SendGridMessage msg;
			if (tos.Count == 1)
			{
				msg = MailHelper.CreateSingleEmail
				(
					from,
					tos[0],
					subject,
					plainTextContent: null,
					htmlContent: body
				);
			}
			else
			{
				msg = MailHelper.CreateSingleEmailToMultipleRecipients
				(
					from,
					tos,
					subject,
					plainTextContent: null,
					htmlContent: body
				);
			}

			var client = new SendGridClient(_apiKey);
			await client.SendEmailAsync(msg);
		}
	}
}
