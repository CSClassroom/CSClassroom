using System;
using System.Linq;

namespace CSC.Common.Infrastructure.Extensions
{
	/// <summary>
	/// Extension methods for the string class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Changes the first character of a string to lower-case, 
		/// if it is not already.
		/// </summary>
		public static string ToCamelCase(this string str)
		{
			if (str.Length == 0)
				return str;

			return str.Substring(0, 1).ToLower() + str.Substring(1);
		}

		/// <summary>
		/// Changes the first character of a string to upper-case, 
		/// if it is not already.
		/// </summary>
		public static string ToPascalCase(this string str)
		{
			if (str.Length == 0)
				return str;

			return str.Substring(0, 1).ToUpper() + str.Substring(1);
		}

		/// <summary>
		/// Returns a version of the string with non-alpha-numeric
		/// characters stripped out.
		/// </summary>
		public static string ToAlphaNumeric(this string str)
		{
			return new string(str.Where(char.IsLetterOrDigit).ToArray());
		}

		/// <summary>
		/// Returns a string where every line in the string has all 
		/// whitespace stripped from either end.
		/// </summary>
		public static string TrimEveryLine(this string str)
		{
			var lines = str.Trim().Split
			(
				new string[] { "\r\n", "\n" },
				StringSplitOptions.None
			);

			return string.Join
			(
				"\n",
				lines.Select(line => line.Trim())
			).Trim();
		}
	}
}
