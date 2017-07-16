using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Core.Enrichers;

namespace CSC.CSClassroom.WebApp.Logging
{
	/// <summary>
	/// Allows for adding additional properties to all log messages
	/// logged within newly created scopes.
	/// </summary>
	public class LogContext : ILogContext
	{
		/// <summary>
		/// Creates a new log scope, with the given properties.
		/// </summary>
		public IDisposable CreateLogScope(IList<KeyValuePair<string, string>> properties)
		{
			return Serilog.Context.LogContext.Push
			(
				properties.Select
				(
					p => new PropertyEnricher(p.Key, p.Value)
				).Cast<ILogEventEnricher>()
				.ToArray()
			);
		}
	}
}
