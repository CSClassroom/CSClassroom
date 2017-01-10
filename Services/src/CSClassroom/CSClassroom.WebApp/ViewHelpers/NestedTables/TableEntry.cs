namespace CSC.CSClassroom.WebApp.ViewHelpers.NestedTables
{
	/// <summary>
	/// A table entry.
	/// </summary>
	public abstract class TableEntry
	{
		/// <summary>
		/// Returns html for a link.
		/// </summary>
		protected string GetLink(string url, string text, bool preventWrapping)
		{
			if (string.IsNullOrEmpty(url))
				return string.Empty;

			var whitespace = preventWrapping ? "nowrap" : "normal;";
			return $"<a href=\"{url}\" style=\"white-space:{whitespace}\" target=\"_blank\">{text}</a>";
		}

		/// <summary>
		/// Returns HTML for colored text.
		/// </summary>
		protected string GetColoredText(string fontColor, string text, bool bold, bool preventWrapping)
		{
			var fontWeight = bold ? "bold" : "normal";
			var whitespace = preventWrapping ? "nowrap" : "normal;";
			return $"<span style=\"color:{fontColor}; font-weight:{fontWeight}; white-space: {whitespace}\">{text}</span>";
		}

		/// <summary>
		/// Returns HTML for bold text.
		/// </summary>
		protected string GetBoldText(string text, bool preventWrapping)
		{
			var whitespace = preventWrapping ? "nowrap" : "normal;";
			return $"<span style=\"font-weight:bold; white-space: {whitespace}\">{text}</span>";
		}
	}
}
