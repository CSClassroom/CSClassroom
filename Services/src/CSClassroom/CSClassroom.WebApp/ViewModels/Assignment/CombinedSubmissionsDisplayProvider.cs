using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Functionality useful for populating assignment view models
	/// for assignments with combined submissions.
	/// </summary>
	public class CombinedSubmissionsDisplayProvider : AssignmentDisplayProvider
	{
		/// <summary>
		/// The assignment result.
		/// </summary>
		private readonly CombinedSubmissionsAssignmentResult _result;
		protected override AssignmentResult Result => _result;

		/// <summary>
		/// Constructor.
		/// </summary>
		public CombinedSubmissionsDisplayProvider(
			ITimeZoneProvider timeZoneProvider, 
			IAssignmentUrlProvider assignmentUrlProvider,
			CombinedSubmissionsAssignmentResult result)
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
			return AssignmentUrlProvider.GetAssignmentUrl
			(
				_result.AssignmentId,
				_result.UserId
			);
		}

		/// <summary>
		/// Returns child table data for the assignment. This could
		/// differ depending on the type of assignment.
		/// </summary>
		public override List<object> GetChildTableData()
		{
			return _result
				.AssignmentSubmissionResults
				.Select
				(
					asr => new AssignmentSubmissionViewModel
					(
						asr,
						AssignmentUrlProvider,
						TimeZoneProvider
					)
				).Cast<object>().ToList();
		}
	}
}
