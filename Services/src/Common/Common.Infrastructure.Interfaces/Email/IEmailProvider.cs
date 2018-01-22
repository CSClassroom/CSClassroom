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
	/// The sender of the e-mail.
	/// </summary>
	public class EmailSender
	{
		/// <summary>
		/// The sender's name.
		/// </summary>
		public string Name { get; }
		
		/// <summary>
		/// The sender's e-mail address.
		/// </summary>
		public string FromAddress { get; }
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public EmailSender(string name, string fromAddress)
		{
			Name = name;
			FromAddress = fromAddress;
		}
	}

	/// <summary>
	/// Information relating to message threading.
	/// </summary>
	public class ThreadInfo
	{
		/// <summary>
		/// A globally unique identifier for the message.
		/// </summary>
		public string MessageId { get; }
		
		/// <summary>
		/// The message ID this message is in reply to.
		/// </summary>
		public string InReplyTo { get; }
		
		/// <summary>
		/// The message IDs that this message references.
		/// </summary>
		public IList<string> References { get; }
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public ThreadInfo(string messageId, string inReplyTo, IList<string> references)
		{
			MessageId = messageId;
			InReplyTo = inReplyTo;
			References = references;
		}
	}

	/// <summary>
	/// Sends mail messages.
	/// </summary>
	public interface IEmailProvider
	{
		/// <summary>
		/// The default from address.
		/// </summary>
		string DefaultFromAddress { get; }

		/// <summary>
		/// Sends a mail message.
		/// </summary>
		Task SendMessageAsync(
			IList<EmailRecipient> recipients,
			string subject,
			string body,
			EmailSender customSender = null,
			ThreadInfo threadInfo = null);
	}
}
