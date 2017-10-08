using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.Async
{
	/// <summary>
	/// Functionality to execute one or more generic operations
	/// with fallback behavior.
	/// </summary>
	public interface IOperationRunner
	{
		/// <summary>
		/// Executes a set of operations in parallel, with a limit
		/// on the maximum number of simultaneous operations.
		/// </summary>
		Task<IList<TResult>> RunOperationsAsync<TSource, TResult>(
			IEnumerable<TSource> sources,
			Func<TSource, Task<TResult>> operation,
			int maxSimultaneous = 5);

		/// <summary>
		/// Executes an operation, retrying the operation if needed.
		/// </summary>
		Task RetryOperationIfNeededAsync(
			Func<Task> operation,
			Func<Exception, bool> shouldRetry,
			int numAttempts,
			TimeSpan delayBetweenRetries,
			bool defaultResultIfFailed);

		/// <summary>
		/// Executes an operation, retrying the operation if needed.
		/// </summary>
		Task<TResult> RetryOperationIfNeededAsync<TResult>(
			Func<Task<TResult>> operation,
			Func<Exception, bool> shouldRetry,
			int numAttempts,
			TimeSpan delayBetweenRetries,
			bool defaultResultIfFailed);

		/// <summary>
		/// Runs a given operation with a timeout. Returns whether the
		/// operation completed within the given amount of time.
		/// </summary>
		Task<bool> RunOperationWithTimeoutAsync(
			Func<Task> operation,
			TimeSpan timeout);
	}
}
