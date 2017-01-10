namespace CSC.CSClassroom.WebApp.Settings
{
	/// <summary>
	/// The host for the site. 
	/// (This is a separate type to enable dependency injection.)
	/// </summary>
	public class WebAppHost
	{
		/// <summary>
		/// The host name of the service from an external source.
		/// </summary>
		public string HostName { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public WebAppHost(string hostName)
		{
			HostName = hostName;
		}
	}
}
