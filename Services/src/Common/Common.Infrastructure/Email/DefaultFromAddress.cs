namespace CSC.Common.Infrastructure.Email
{
	/// <summary>
	/// The default e-mail address that messages will be sent from.
	/// (This is a separate type to enable dependency injection.)
	/// </summary>
	public class DefaultFromAddress
	{
		/// <summary>
		/// The e-mail address that messages will be sent from.
		/// </summary>
		public string EmailAddress { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public DefaultFromAddress(string emailAddress)
		{
			EmailAddress = emailAddress;
		}
	}
}
