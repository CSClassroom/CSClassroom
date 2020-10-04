using System;
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
		/// The Postmark config.
		/// </summary>
		private readonly PostmarkEmailProviderConfig _config;

		/// <summary>
		/// Constructor.
		/// </summary>
		public PostmarkEmailProvider(
			ILogger<PostmarkEmailProvider> logger,
			PostmarkEmailProviderConfig config)
		{
			_logger = logger;
			_config = config;
		}

		/// <summary>
		/// The default e-mail address to send e-mails from, unless overridden.
		/// </summary>
		public string DefaultFromAddress
		{
			get { return _config.DefaultFromEmail; }
		}

		/// <summary>
		/// Send a mail message.
		/// </summary>
		public async Task SendMessageAsync(
			IList<EmailRecipient> recipients,
			string subject,
			string body,
			bool broadcast,
			EmailSender customSender = null,
			ThreadInfo threadInfo = null)
		{
			var client = new PostmarkClient(_config.ApiKey);
			var responses = await client.SendMessagesAsync
			(
				recipients.Select
				(
					recipient => CreateMessage
					(
						recipient,
						subject,
						body,
						broadcast,
						customSender,
						threadInfo
					)
				)
			);

			foreach (var response in responses)
            {
				if (response.Status != PostmarkStatus.Success)
				{
					_logger.Log
					(
						0,
						response.Message,
						"Failed to send e-mail to {toEmail} with subject {subject}",
						response.To,
						subject
					);
				}
			}
		}

		/// <summary>
		/// Creates a new Postmark message for the given recipient.
		/// </summary>
		private PostmarkMessage CreateMessage(
			EmailRecipient recipient,
			string subject,
			string body,
			bool broadcast,
			EmailSender customSender = null,
			ThreadInfo threadInfo = null)
        {
			var message = new PostmarkMessage()
			{
				To = $"{recipient.Name} {recipient.EmailAddress}",
				From = customSender != null
					? $"{customSender.Name} {customSender.FromAddress}"
					: $"CS Classroom {_config.DefaultFromEmail}",
				TrackOpens = true,
				Subject = subject,
				HtmlBody = body,
				MessageStream = broadcast
					? _config.BroadcastMessageStream
					: _config.TransactionalMessageStream
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

			return message;
		}
	}

	/// <summary>
	/// The configuration for the Postmark e-mail provider
	/// </summary>
	public class PostmarkEmailProviderConfig
	{
		/// <summary>
		/// The API key to use with Postmark.
		/// </summary>
		public string ApiKey { get; }

		/// <summary>
		/// The e-mail address that messages will be sent from.
		/// </summary>
		public string DefaultFromEmail { get; }

		/// <summary>
		/// The message stream to use for transactional messags to a single user.
		/// </summary>
		public string TransactionalMessageStream { get; }

		/// <summary>
		/// The message stream to use for broadcast messages (i.e. announcements).
		/// </summary>
		public string BroadcastMessageStream { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PostmarkEmailProviderConfig(
			string apiKey,
			string defaultFromEmail,
			string transactionalMessageStream,
			string broadcastMessageStream)
		{
			ApiKey = apiKey
				?? throw new ArgumentNullException(nameof(apiKey));
			DefaultFromEmail = defaultFromEmail
				?? throw new ArgumentNullException(nameof(defaultFromEmail));
			TransactionalMessageStream = transactionalMessageStream
				?? throw new ArgumentNullException(nameof(transactionalMessageStream));
			BroadcastMessageStream = broadcastMessageStream
				?? throw new ArgumentNullException(nameof(broadcastMessageStream));
		}
	}
}
