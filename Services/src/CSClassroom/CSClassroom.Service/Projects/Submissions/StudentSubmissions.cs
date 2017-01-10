using System.Collections.Generic;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// The contents of all student submissions.
	/// </summary>
	public class StudentSubmissions : List<StudentSubmission>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentSubmissions(IList<StudentSubmission> submissions) 
			: base(submissions)
		{
		}

		/// <summary>
		/// Disposes all student submissions.
		/// </summary>
		public void Dispose()
		{
			foreach (var submission in this)
			{
				submission.Dispose();
			}
		}
	}
}
