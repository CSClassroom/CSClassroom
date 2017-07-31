using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// The result of a grade operation.
	/// </summary>
	public class GradeSubmissionResult
	{
		/// <summary>
		/// The scored question result, if it should be returned to the client.
		/// </summary>
		public ScoredQuestionResult ScoredQuestionResult { get; set; }

		/// <summary>
		/// The submission date, if the user should be redirected to the submission.
		/// </summary>
		public DateTime? SubmissionDate { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GradeSubmissionResult(ScoredQuestionResult scoredQuestionResult)
		{
			ScoredQuestionResult = scoredQuestionResult;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public GradeSubmissionResult(DateTime submissionDate)
		{
			SubmissionDate = submissionDate;
		}
	}
}
