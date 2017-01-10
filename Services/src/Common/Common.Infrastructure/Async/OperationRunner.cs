using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CSC.Common.Infrastructure.Async
{
	/// <summary>
	/// Functionality to execute one or more generic operations
	/// with fallback behavior.
	/// </summary>
	public class OperationRunner : IOperationRunner
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<OperationRunner> _logger;

		/// <summary>
		/// Constructor.
		/// </summary>
		public OperationRunner(ILogger<OperationRunner> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Executes a set of operations in parallel, with a limit
		/// on the maximum number of simultaneous operations.
		/// </summary>
		public async Task<IList<TResult>> RunOperationsAsync<TSource, TResult>(
			IEnumerable<TSource> sources,
			Func<TSource, Task<TResult>> operation,
			int maxSimultaneous)
		{
			var currentlyRunningTasks = sources
				.Select(operation)
				.Take(maxSimultaneous)
				.ToList();

			var sourcesWaitingToStart = sources
				.Skip(maxSimultaneous)
				.ToList();

			var results = new List<TResult>();

			while (sourcesWaitingToStart.Any())
			{
				var completedTask = await Task.WhenAny(currentlyRunningTasks);
				results.Add(completedTask.Result);
				currentlyRunningTasks.Remove(completedTask);

				var nextSource = sourcesWaitingToStart.First();
				sourcesWaitingToStart.Remove(nextSource);
				currentlyRunningTasks.Add(operation(nextSource));
			}

			var remainingResults = await Task.WhenAll(currentlyRunningTasks);
			results.AddRange(remainingResults);

			return results;
		}

		/// <summary>
		/// Executes an operation, retrying the operation if needed.
		/// </summary>
		public async Task<TResult> RetryOperationIfNeededAsync<TResult>(
			Func<Task<TResult>> operation,
			Func<Exception, bool> shouldRetry,
			int numAttempts,
			TimeSpan delayBetweenRetries,
			bool defaultResultIfFailed)
		{
			for (int attempt = 1; attempt <= numAttempts; attempt++)
			{
				try
				{
					return await operation();
				}
				catch (Exception ex)
				{
					var tryAgain = attempt < numAttempts && shouldRetry(ex);

					_logger.LogWarning
					(
						0,
						ex,
						tryAgain
							? "Failed operation on attempt {attempt}. Retrying."
							: "Permanently failed on attenpt {attempt}.",
						attempt
					);

					if (!tryAgain)
					{
						if (defaultResultIfFailed)
						{
							return default(TResult);
						}
						else
						{
							throw;
						}
					}
				}
			}

			throw new InvalidOperationException("Not reachable.");
		}

		/// <summary>
		/// Runs a given operation with a timeout. Returns whether the
		/// operation completed within the given amount of time.
		/// </summary>
		public async Task<bool> RunOperationWithTimeoutAsync(
			Func<Task> operation,
			TimeSpan timeout)
		{
			var operationTask = operation();
			var timeoutTask = Task.Delay(timeout);

			var taskFinishedFirst = await Task.WhenAny(operationTask, timeoutTask);

			return taskFinishedFirst == operationTask;
		}
	}
}
