using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.Common.Infrastructure.Interfaces.Extensions
{
	/// <summary>
	/// Extension methods for a dictionary.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Returns a value from the given dictionary if it exists,
		/// or a new default-constructed object if it does not.
		/// </summary>
		public static TValue GetValueOrNew<TKey, TValue>(
			this IDictionary<TKey, TValue> resultsByKey,
			TKey key) where TValue: new()
		{
			if (resultsByKey.ContainsKey(key))
				return resultsByKey[key];
			else
				return new TValue();
		}

		/// <summary>
		/// Returns a value from the given dictionary if it exists,
		/// or the default value if it does not.
		/// </summary>
		public static TValue GetValueOrDefault<TKey, TValue>(
			this IDictionary<TKey, TValue> resultsByKey,
			TKey key)
		{
			if (resultsByKey.ContainsKey(key))
				return resultsByKey[key];
			else
				return default(TValue);
		}
	}
}
