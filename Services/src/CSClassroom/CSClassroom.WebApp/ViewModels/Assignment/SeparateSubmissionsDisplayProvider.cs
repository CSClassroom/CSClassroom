using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Functionality useful for populating assignment view models
	/// for assignments with separate submissions for each question.
	/// </summary>
	public class SeparateSubmissionsDisplayProvider : AssignmentDisplayProvider
	{
		/// <summary>
		/// The assignment result.
		/// </summary>
		private readonly SeparateSubmissionsAssignmentResult _result;
		protected override AssignmentResult Result => _result;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SeparateSubmissionsDisplayProvider(
			ITimeZoneProvider timeZoneProvider,
			IAssignmentUrlProvider assignmentUrlProvider,
			SeparateSubmissionsAssignmentResult result)
			: base(timeZoneProvider, assignmentUrlProvider)
		{
			_result = result;
		}

		/// <summary>
		/// Returns the URL to link to for this assignment
		/// (or null if no such URL should be linked to).
		/// </summary>
		public override string GetAssignmentUrl()
		{
			return null;
		}

		/// <summary>
		/// Returns child table data for the assignment. This could
		/// differ depending on the type of assignment.
		/// </summary>
		public override List<object> GetChildTableData()
		{
			return _result
				.QuestionResults
				.Select
				(
					qr => new StudentQuestionResultViewModel
					(
						qr,
						AssignmentUrlProvider,
						TimeZoneProvider
					)
				).Cast<object>().ToList();
		}
	}
}
