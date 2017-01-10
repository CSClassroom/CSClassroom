using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;

namespace CSC.Common.TestDoubles
{
	/// <summary>
	/// A mock version of the operation runner.
	/// </summary>
	public class MockOperationRunner : IOperationRunner
	{
		/// <summary>
		/// Executes a set of operations in parallel, with a limit
		/// on the maximum number of simultaneous operations.
		/// </summary>
		public async Task<IList<TResult>> RunOperationsAsync<TSource, TResult>(
			IEnumerable<TSource> sources,
			Func<TSource, Task<TResult>> operation,
			int maxSimultaneous = 5)
		{
			var results = new List<TResult>();

			foreach (var source in sources)
			{
				results.Add(await operation(source));
			}

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
			try
			{
				return await operation();
			}
			catch (Exception)
			{
				if (defaultResultIfFailed)
				{
					return default(TResult);
				}

				throw;
			}
		}

		/// <summary>
		/// Runs a given operation with a timeout. Returns whether the
		/// operation completed within the given amount of time.
		/// </summary>
		public async Task<bool> RunOperationWithTimeoutAsync(
			Func<Task> operation,
			TimeSpan timeout)
		{
			await operation();

			return true;
		}
	}
}
