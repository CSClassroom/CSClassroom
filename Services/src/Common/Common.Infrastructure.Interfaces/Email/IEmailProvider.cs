using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.Email
{
	/// <summary>
	/// Sends mail messages.
	/// </summary>
	public interface IEmailProvider
	{
		/// <summary>
		/// Send a mail message.
		/// </summary>
		Task SendMessageAsync(
			string toAddress,
			string fromAddress,
			string fromName,
			string subject,
			string body);
	}
}
