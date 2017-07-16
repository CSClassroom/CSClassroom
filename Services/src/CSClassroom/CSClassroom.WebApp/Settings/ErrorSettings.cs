namespace CSC.CSClassroom.WebApp.Settings
{
	/// <summary>
	/// Whether or not to show full error information.
	/// </summary>
	public class ErrorSettings
	{
		/// <summary>
		/// Whether or not to show full exceptions for errors.
		/// </summary>
		public bool ShowExceptions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ErrorSettings(bool showExceptions)
		{
			ShowExceptions = showExceptions;
		}
	}
}
