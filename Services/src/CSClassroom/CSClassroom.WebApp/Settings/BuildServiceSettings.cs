namespace CSC.CSClassroom.WebApp.Settings
{
	/// <summary>
	/// Settings for the code runner service.
	/// </summary>
	public class BuildServiceSettings
	{
		/// <summary>
		/// The host of the build service.
		/// </summary>
		public string Host { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BuildServiceSettings(string host)
		{
			Host = host;
		}
	}
}
