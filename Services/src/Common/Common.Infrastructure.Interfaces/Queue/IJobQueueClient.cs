using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.Queue
{
	/// <summary>
	/// A client to manage jobs in a job queue.
	/// </summary>
	public interface IJobQueueClient
	{
		/// <summary>
		/// Enqueues a job.
		/// </summary>
		/// <typeparam name="TInterface">An interface containing the method to run.</typeparam>
		/// <param name="job">A call to a method on the given interface.
		/// All parameters to the method must be serializable.</param>
		/// <returns>The job ID.</returns>
		Task<string> EnqueueAsync<TInterface>(Expression<Func<TInterface, Task>> job);

		/// <summary>
		/// Returns the status of the job with the given ID.
		/// </summary>
		Task<JobStatus> GetJobStatusAsync(string jobId);
	}
}
