namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// The configuration for a sub panel.
	/// </summary>
	public class SubPanelConfig
	{
		/// <summary>
		/// The name of the property containing the sub panel contents.
		/// </summary>
		public string ContentsPropertyName { get; }

		/// <summary>
		/// The name of the sub panel builder function.
		/// </summary>
		public string BuilderFunctionName { get; }

		/// <summary>
		/// The name of the row data loaded function.
		/// </summary>
		public string LoadedFunctionName { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubPanelConfig(string contentsPropertyName, string builderFunctionName, string loadedFunctionName)
		{
			ContentsPropertyName = contentsPropertyName;
			BuilderFunctionName = builderFunctionName;
			LoadedFunctionName = loadedFunctionName;
		}
	}
}
