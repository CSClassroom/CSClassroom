using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.Queue
{
	/// <summary>
	/// The status of a job in the queue.
	/// </summary>
	public class JobStatus
	{
		/// <summary>
		/// The state of the job.
		/// </summary>
		public JobState State { get; }

		/// <summary>
		/// The time at which the job entered 
		/// its current state.
		/// </summary>
		public DateTime EnteredState { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public JobStatus(JobState state, DateTime enteredState)
		{
			State = state;
			EnteredState = enteredState;
		}
	}
}
