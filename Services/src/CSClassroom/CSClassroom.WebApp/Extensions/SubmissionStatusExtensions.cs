using System;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.WebApp.Extensions
{
	/// <summary>
	/// Extension methods for the submission status class.
	/// </summary>
	public static class SubmissionStatusExtensions
	{
		/// <summary>
		/// The status color for this question.
		/// </summary>
		public static string GetColor(this SubmissionStatus status)
		{
			switch (status.Completion)
			{
				case Completion.Completed:
					return "green";

				case Completion.InProgress:
				case Completion.NotStarted:
					return status.Late ? "red" : "orange";

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// The status text for this question.
		/// </summary>
		public static string GetText(this SubmissionStatus status)
		{
			var lateSuffix = status.Late ? " (Late)" : "";

			switch (status.Completion)
			{
				case Completion.Completed:
					return $"Completed{lateSuffix}";

				case Completion.InProgress:
					return $"In Progress{lateSuffix}";

				case Completion.NotStarted:
					return $"Not Started{lateSuffix}";

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Returns whether or not the text should be bold.
		/// </summary>
		public static bool GetBold(this SubmissionStatus status)
		{
			switch (status.Completion)
			{
				case Completion.Completed:
					return false;

				case Completion.InProgress:
				case Completion.NotStarted:
					return status.Late;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
