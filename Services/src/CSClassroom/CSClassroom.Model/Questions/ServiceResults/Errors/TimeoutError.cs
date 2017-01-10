namespace CSC.CSClassroom.Model.Questions.ServiceResults.Errors
{
	/// <summary>
	/// An error indicating the job took too long.
	/// </summary>
	public class TimeoutError : JobExecutionError
	{
		/// <summary>
		/// The full text for the error.
		/// </summary>
		public override string FullErrorText => "The code did not finish executing in the time alloted.";
	}
}
