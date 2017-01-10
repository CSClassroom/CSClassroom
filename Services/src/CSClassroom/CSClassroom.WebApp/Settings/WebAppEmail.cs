namespace CSC.CSClassroom.WebApp.Settings
{
	/// <summary>
	/// The e-mail address that messages will be sent from.
	/// (This is a separate type to enable dependency injection.)
	/// </summary>
	public class WebAppEmail
	{
		/// <summary>
		/// The e-mail address that messages will be sent from.
		/// </summary>
		public string EmailAddress { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public WebAppEmail(string emailAddress)
		{
			EmailAddress = emailAddress;
		}
	}
}
