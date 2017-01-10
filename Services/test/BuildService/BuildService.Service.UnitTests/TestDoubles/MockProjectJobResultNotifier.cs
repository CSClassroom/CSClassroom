using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.BuildService.Service.ProjectRunner;

namespace CSC.BuildService.Service.UnitTests.TestDoubles
{
	/// <summary>
	/// Simulates a notifier of project job completion.
	/// </summary>
	public class MockProjectJobResultNotifier : IProjectJobResultNotifier
	{
		/// <summary>
		/// Whether or not a notification was made.
		/// </summary>
		public bool Notified { get; private set; }

		/// <summary>
		/// The host to call back.
		/// </summary>
		public string CallbackHost { get; private set; }

		/// <summary>
		/// The path to call back.
		/// </summary>
		public string CallbackPath { get; private set; }

		/// <summary>
		/// The operation ID to send when making the callback.
		/// </summary>
		public string OperationId { get; private set; }

		/// <summary>
		/// The result to notify of.
		/// </summary>
		public ProjectJobResult Result { get; private set; }

		/// <summary>
		/// Simulates a notification to the source of the project job 
		/// that the job is complete.
		/// </summary>
		public Task NotifyAsync(
			string callbackHost,
			string callbackPath,
			string operationId,
			ProjectJobResult result)
		{
			Notified = true;
			CallbackHost = callbackHost;
			CallbackPath = callbackPath;
			OperationId = operationId;
			Result = result;

			return Task.CompletedTask;
		}
	}
}
