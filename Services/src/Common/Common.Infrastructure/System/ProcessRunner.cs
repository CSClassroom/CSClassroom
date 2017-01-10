using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// Runs a process.
	/// </summary>
	public class ProcessRunner : IProcessRunner
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger _logger;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProcessRunner(ILogger<ProcessRunner> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Runs a process, optionally killing the process if it has not completed
		/// in the given timeout. Returns the combined contents of stdout/stderr,
		/// along with whether the job completed in the given timeout.
		/// </summary>
		public async Task<ProcessResult> RunProcessAsync(
			string path,
			string[] args,
			TimeSpan? timeout)
		{
			var output = new ConcurrentQueue<string>();
			var processTcs = new TaskCompletionSource<bool>();

			var process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = path,
					Arguments = string.Join(" ", args),
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				},

				EnableRaisingEvents = true
			};

			process.OutputDataReceived += (sender, e) =>
			{
				output.Enqueue(e.Data);
			};

			process.ErrorDataReceived += (sender, e) =>
			{
				output.Enqueue(e.Data);
			};

			process.Exited += (sender, e) =>
			{
				processTcs.TrySetResult(true);
			};

			_logger.LogInformation("Starting process {processName} with arguments {arguments}.",
				process.StartInfo.FileName,
				process.StartInfo.Arguments);

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			if (timeout != null)
			{
				await Task.WhenAny(processTcs.Task, Task.Delay(timeout.Value));
			}
			else
			{
				await processTcs.Task;
			}

			var completed = process.HasExited;
			if (!completed)
			{
				process.Kill();
			}

			var outputStr = string.Join("\n", output);
			var truncatedOutputStr = outputStr.Length > 1000
				? outputStr.Substring(outputStr.Length - 1000)
				: outputStr;

			_logger.LogInformation
			(
				  "Process {processName} with arguments {arguments} finished "
				+ "with status {status}. Output: \n {output}",
				process.StartInfo.FileName,
				process.StartInfo.Arguments,
				completed ? "Completed" : "Timeout",
				truncatedOutputStr
			);

			return new ProcessResult(completed, outputStr);
		}
	}
}
