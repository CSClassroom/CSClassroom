using System.Net.Mail;
using System.Threading.Tasks;
using SendGrid;

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
		private string _apiKey;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SendGridEmailProvider(string apiKey)
		{
			_apiKey = apiKey;
		}

		/// <summary>
		/// Send a mail message.
		/// </summary>
		public async Task SendMessageAsync(
			string toAddress, 
			string fromAddress, 
			string fromName, 
			string subject, 
			string body)
		{
			if (_apiKey == null)
			{
				// E-mail is disabled.
				return;
			}

			var sendGridMessage = new SendGridMessage();
			sendGridMessage.AddTo(toAddress);
			sendGridMessage.From = new MailAddress(fromAddress, fromName);
			sendGridMessage.Subject = subject;
			sendGridMessage.Html = body;

			var transportWeb = new Web(_apiKey);

			await transportWeb.DeliverAsync(sendGridMessage);
		}
	}
}
