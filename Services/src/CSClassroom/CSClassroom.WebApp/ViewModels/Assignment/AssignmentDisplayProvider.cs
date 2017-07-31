using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Functionality useful for populating assignment view models.
	/// </summary>
	public abstract class AssignmentDisplayProvider : IAssignmentDisplayProvider
	{
		/// <summary>
		/// Provides time zone information.
		/// </summary>
		protected ITimeZoneProvider TimeZoneProvider { get; }

		/// <summary>
		/// Provides URLs relating to assignments.
		/// </summary>
		protected IAssignmentUrlProvider AssignmentUrlProvider { get; }

		/// <summary>
		/// The assignment result.
		/// </summary>
		protected abstract AssignmentResult Result { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AssignmentDisplayProvider(
			ITimeZoneProvider timeZoneProvider, 
			IAssignmentUrlProvider assignmentUrlProvider)
		{
			TimeZoneProvider = timeZoneProvider;
			AssignmentUrlProvider = assignmentUrlProvider;
		}

		/// <summary>
		/// Returns the URL to link to for this assignment
		/// (or null if no such URL should be linked to).
		/// </summary>
		public abstract string GetAssignmentUrl();

		/// <summary>
		/// Returns child table data for the assignment. This could
		/// differ depending on the type of assignment.
		/// </summary>
		public abstract List<object> GetChildTableData();

		/// <summary>
		/// Returns the assignment due date text.
		/// </summary>
		public string GetAssignmentDueDate()
		{
			return Result.AssignmentDueDate
				?.FormatLongDateTime(TimeZoneProvider)
				?? "Unassigned";
		}
	}
}
