using System;
using System.Collections.Generic;
using System.Text;
using Ganss.XSS;

namespace CSC.Common.Infrastructure.Security
{
	/// <summary>
	/// Performs sanitization on an HTML string.
	/// </summary>
	public class HtmlSanitizer : IHtmlSanitizer
	{
		/// <summary>
		/// Sanitizes an HTML string.
		/// </summary>
		public string SanitizeHtml(string html)
		{
			var sanitizer = new Ganss.XSS.HtmlSanitizer();
			return sanitizer.Sanitize(html);
		}
	}
}
