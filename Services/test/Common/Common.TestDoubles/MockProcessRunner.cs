using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;

namespace CSC.Common.TestDoubles
{
	/// <summary>
	/// Represents a launch of a process by the 
	/// mock process runner.
	/// </summary>
	public class ProcessLaunch
	{
		/// <summary>
		/// The path to the process that was run.
		/// </summary>
		public string ProcessPath { get; }

		/// <summary>
		/// Command-line arguments passed to the process.
		/// </summary>
		public string[] Args { get; }

		/// <summary>
		/// The timeout passed to the process runner.
		/// </summary>
		public TimeSpan? Timeout { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProcessLaunch(
			string processPath, 
			string[] args, 
			TimeSpan? timeout)
		{
			ProcessPath = processPath;
			Args = args;
			Timeout = timeout;
		}
	}

	/// <summary>
	/// Simulates running a process.
	/// </summary>
	public class MockProcessRunner : IProcessRunner
	{
		/// <summary>
		/// Stores requests to launch a process.
		/// </summary>
		public IList<ProcessLaunch> Launches { get; }
		
		/// <summary>
		/// The process to simulate.
		/// </summary>
		private readonly Queue<Func<Task<ProcessResult>>> _simulatedProcesses;

		/// <summary>
		/// Constructor.
		/// </summary>
		public MockProcessRunner(params Func<Task<ProcessResult>>[] simulatedProcesses)
		{
			Launches = new List<ProcessLaunch>();

			_simulatedProcesses = new Queue<Func<Task<ProcessResult>>>
			(
				simulatedProcesses
			);
		}

		/// <summary>
		/// Runs a process. Returns the combined contents of stdout/stderr
		/// if desired.
		/// </summary>
		public async Task<ProcessResult> RunProcessAsync(
			string path,
			string[] args,
			TimeSpan? timeout)
		{
			Launches.Add(new ProcessLaunch(path, args, timeout));

			return await _simulatedProcesses.Dequeue()();
		}
	}
}
