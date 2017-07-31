using System;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// A previous submission to a question.
	/// </summary>
	public class SubmissionResult
	{
		/// <summary>
		/// The solved question.
		/// </summary>
		public QuestionToSolve QuestionSubmitted { get; set; }

		/// <summary>
		/// The question result.
		/// </summary>
		public QuestionResult QuestionResult { get; set; }

		/// <summary>
		/// The score for this question without lateness
		/// </summary>
		public double ScoreWithoutLateness { get; set; }

		/// <summary>
		/// The score for this question with lateness
		/// </summary>
		public double ScoreWithLateness { get; set; }

		/// <summary>
		/// The number of points for the question.
		/// </summary>
		public double QuestionPoints { get; set; }

		/// <summary>
		/// The date/time of the submission.
		/// </summary>
		public DateTime SubmissionDate { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionResult(
			QuestionToSolve questionSubmitted, 
			QuestionResult questionResult, 
			double scoreWithoutLateness,
			double scoreWithLateness,
			double questionPoints,
			DateTime submissionDate)
		{
			QuestionSubmitted = questionSubmitted;
			QuestionResult = questionResult;
			ScoreWithoutLateness = scoreWithoutLateness;
			ScoreWithLateness = scoreWithLateness;
			QuestionPoints = questionPoints;
			SubmissionDate = submissionDate;
		}
	}
}
