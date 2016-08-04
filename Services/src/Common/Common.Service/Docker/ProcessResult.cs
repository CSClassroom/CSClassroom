using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// The result of executing a process.
	/// </summary>
    public class ProcessResult
    {
		/// <summary>
		/// Whether or not the successfully completed without timing out.
		/// </summary>
		public bool Completed { get; private set; }

		/// <summary>
		/// The combined contents of the stdout/stderr streams of the process.
		/// </summary>
		public string Output { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProcessResult(bool completed, string output)
		{
			Completed = completed;
			Output = output;
		}
    }
}
