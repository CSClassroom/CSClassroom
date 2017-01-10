using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// Runs a process.
	/// </summary>
	public interface IProcessRunner
	{
		/// <summary>
		/// Runs a process, optionally killing the process if it has not completed
		/// in the given timeout. Returns the combined contents of stdout/stderr,
		/// along with whether the job completed in the given timeout.
		/// </summary>
		Task<ProcessResult> RunProcessAsync(
			string path,
			string[] args,
			TimeSpan? timeout);
	}
}
