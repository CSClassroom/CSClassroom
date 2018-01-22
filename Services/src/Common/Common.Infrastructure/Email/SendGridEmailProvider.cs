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
		/// The default from address for all messages.
		/// </summary>
		public string DefaultFromAddress { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SendGridEmailProvider(string apiKey, string defaultFromAddress)
		{
			_apiKey = apiKey;
			DefaultFromAddress = defaultFromAddress;
		}

		/// <summary>
		/// Send a mail message.
		/// </summary>
		public async Task SendMessageAsync(
			IList<EmailRecipient> recipients,
			string subject, 
			string body,
			EmailSender customSender = null,
			ThreadInfo threadInfo = null)
		{
			if (_apiKey == null)
			{
				// E-mail is disabled.
				return;
			}

			var from = customSender != null
				? new EmailAddress(customSender.FromAddress, customSender.Name)
				: new EmailAddress(DefaultFromAddress, "CS Classroom");
			var tos = recipients
				.Select(r => new EmailAddress(r.EmailAddress, r.Name))
				.ToList();
			
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

			if (threadInfo != null)
			{
				msg.Headers = new Dictionary<string, string>();
				
				msg.Headers["Message-Id"] = $"<{threadInfo.MessageId}>";
				
				if (threadInfo.InReplyTo != null)
				{
					msg.Headers["In-Reply-To"] = $"<{threadInfo.InReplyTo}";
				}

				if (threadInfo.References != null)
				{
					msg.Headers["References"] = string.Join
					(
						" ", 
						threadInfo.References.Select(r => $"<{r}>")
					);
				}
			}

			var client = new SendGridClient(_apiKey);
			await client.SendEmailAsync(msg);
		}
	}
}
