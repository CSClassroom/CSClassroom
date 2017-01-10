namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// Settings for the markdown editor.
	/// </summary>
	public class MarkdownEditorSettings
	{
		/// <summary>
		/// The unique editor name.
		/// </summary>
		public string EditorName { get; }

		/// <summary>
		/// The name of the text area to keep in sync with the editor.
		/// </summary>
		public string TextAreaName { get; }

		/// <summary>
		/// The initial contents of the editor, in markdown.
		/// </summary>
		public string InitialContents { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MarkdownEditorSettings(string editorName, string textAreaName, string initialContents)
		{
			EditorName = editorName;
			TextAreaName = textAreaName;
			InitialContents = initialContents;
		}
	}
}
