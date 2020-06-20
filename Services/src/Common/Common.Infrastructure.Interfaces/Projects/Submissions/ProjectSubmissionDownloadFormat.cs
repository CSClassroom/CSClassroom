namespace CSC.Common.Infrastructure.Projects.Submissions
{
	/// <summary>
	/// The category of submission components to download.  This enum is used by
	/// the view model, submission service, and the submission archive builder
	/// </summary>
	public enum ProjectSubmissionDownloadFormat
	{
		// Flat file list for use when doing a plagiarism check 
		Flat,

		// Full eclipse project for use when running students' code
		Eclipse,

		// All of the above
		All,
	}
}