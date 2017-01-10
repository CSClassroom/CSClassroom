using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.WebApp.Logging
{
	/// <summary>
	/// Allows for adding additional properties to all log messages
	/// logged within newly created scopes.
	/// </summary>
	public interface ILogContext
	{
		/// <summary>
		/// Creates a new log scope, with the given properties.
		/// </summary>
		IDisposable CreateLogScope(IList<KeyValuePair<string, string>> properties);
	}
}
