using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PostmarkDotNet;
using PostmarkDotNet.Model;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CSC.Common.Infrastructure.Email
{
	/// <summary>
	/// Sends mail messages.
	/// </summary>
	public class PostmarkEmailProvider : IEmailProvider
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<PostmarkEmailProvider> _logger;

		/// <summary>
		/// The Postmark API key.
		/// </summary>
		private readonly string _apiKey;

		/// <summary>
		/// The default from address for all messages.
		/// </summary>
		public string DefaultFromAddress { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PostmarkEmailProvider(
			ILogger<PostmarkEmailProvider> logger,
			PostmarkApiKey postmarkApiKey,
			DefaultFromAddress defaultFromAddress)
		{
			_logger = logger;
			_apiKey = postmarkApiKey.ApiKey;
			DefaultFromAddress = defaultFromAddress.EmailAddress;
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

			var message = new PostmarkMessage()
			{
				To = string.Join
				(
					",",
					recipients.Select(r => $"{r.Name} {r.EmailAddress}")
				),
				From = customSender != null
					? $"{customSender.Name} {customSender.FromAddress}"
					: $"CS Classroom {DefaultFromAddress}",
				TrackOpens = true,
				Subject = subject,
				HtmlBody = body
			};

			if (threadInfo != null)
			{
				var headers = new Dictionary<string, string>();

				headers["Message-Id"] = $"<{threadInfo.MessageId}>";

				if (threadInfo.InReplyTo != null)
				{
					headers["In-Reply-To"] = $"<{threadInfo.InReplyTo}>";
				}

				if (threadInfo.References != null)
				{
					headers["References"] = string.Join
					(
						" ",
						threadInfo.References.Select(r => $"<{r}>")
					);
				}

				message.Headers = new HeaderCollection(headers);
			}

			var client = new PostmarkClient(_apiKey);
			var response = await client.SendMessageAsync(message);
			if (response.Status != PostmarkStatus.Success)
            {
				_logger.Log
				(
					0,
					response.Message,
					"Failed to send e-mail from {fromEmail} to {toEmail} with subject {subject}",
					message.From,
					message.To,
					message.Subject
				);
            }
		}
	}
}
