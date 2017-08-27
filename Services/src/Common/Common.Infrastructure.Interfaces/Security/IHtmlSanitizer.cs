using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.Common.Infrastructure.Security
{
	/// <summary>
	/// Performs sanitization on an HTML string.
	/// </summary>
	public interface IHtmlSanitizer
	{
		/// <summary>
		/// Sanitizes an HTML string.
		/// </summary>
		string SanitizeHtml(string html);
	}
}
