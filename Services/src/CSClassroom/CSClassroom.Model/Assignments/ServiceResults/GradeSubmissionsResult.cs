using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result of a grade operation for multiple questions.
	/// </summary>
	public class GradeSubmissionsResult
	{
		/// <summary>
		/// The submission date.
		/// </summary>
		public DateTime SubmissionDate { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GradeSubmissionsResult(DateTime submissionDate)
		{
			SubmissionDate = submissionDate;
		}
	}
}
