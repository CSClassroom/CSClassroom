namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// Settings for the markdown viewer.
	/// </summary>
	public class MarkdownViewerSettings
	{
		/// <summary>
		/// The unique viewer name.
		/// </summary>
		public string ViewerName { get; }

		/// <summary>
		/// The initial contents of the editor, in markdown.
		/// </summary>
		public string InitialContents { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MarkdownViewerSettings(string viewerName, string initialContents)
		{
			ViewerName = viewerName;
			InitialContents = initialContents;
		}
	}
}
