using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// The list of past submissions for a question by a given user.
	/// </summary>
	public class PastSubmissions
	{
		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// The times of the user's previous submissions.
		/// </summary>
		public IList<DateTime> SubmissionTimes { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PastSubmissions(int userId, IList<DateTime> submissionTimes)
		{
			UserId = userId;
			SubmissionTimes = submissionTimes;
		}
	}
}
