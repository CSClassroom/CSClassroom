using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.Extensions
{
	/// <summary>
	/// Extsension methods for an HTML helper.
	/// </summary>
	public static class IHtmlHelperExtensions
	{
		/// <summary>
		/// Returns a link.
		/// </summary>
		public static HtmlString Link(this IHtmlHelper htmlHelper, string url, string text)
		{
			return new HtmlString($"<a href=\"{url}\">{text}</a>");
		}
	}
}
