namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// Settings for the code editor.
	/// </summary>
	public class CodeEditorSettings
	{
		/// <summary>
		/// The unique editor name.
		/// </summary>
		public string EditorName { get; }

		/// <summary>
		/// The initial contents of the editor.
		/// </summary>
		public string InitialContents { get; }

		/// <summary>
		/// The contents to revert to if revert is selected.
		/// If this is null, no revert option will be present.
		/// </summary>
		public string RevertContents { get; }

		/// <summary>
		/// Whether or not to create a text area (with the same name as the editor name).
		/// </summary>
		public bool TextArea { get; }

		/// <summary>
		/// The minimum number of lines to show.
		/// </summary>
		public int MinLines { get; }

		/// <summary>
		/// The maximum number of lines to show.
		/// </summary>
		public int MaxLines { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CodeEditorSettings(
			string editorName, 
			string initialContents, 
			string revertContents, 
			bool textArea, 
			int minLines, 
			int maxLines)
		{
			EditorName = editorName;
			InitialContents = initialContents;
			RevertContents = revertContents;
			TextArea = textArea;
			MinLines = minLines;
			MaxLines = maxLines;
		}
	}
}
