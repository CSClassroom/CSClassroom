namespace CSC.CSClassroom.Model.Assignments.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating the job could not be run.
	/// </summary>
	public class DiagnosticError : JobExecutionError
	{
		/// <summary>
		/// The diagnostic output for the job.
		/// </summary>
		public string DiagnosticOutput { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public DiagnosticError(string diagnosticOutput)
		{
			DiagnosticOutput = diagnosticOutput;
		}

		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => DiagnosticOutput;
	}
}
