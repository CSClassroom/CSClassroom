using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.Queue
{
	/// <summary>
	/// The state of a job in the queue.
	/// </summary>
	public enum JobState
	{
		NotFound,
		NotStarted,
		InProgress,
		Completed,
		Unknown
	}
}
