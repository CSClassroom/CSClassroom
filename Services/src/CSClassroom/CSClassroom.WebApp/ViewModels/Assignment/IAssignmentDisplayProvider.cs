using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Functionality useful for populating assignment view models.
	/// </summary>
	public interface IAssignmentDisplayProvider
	{
		/// <summary>
		/// Returns the assignment due date text.
		/// </summary>
		string GetAssignmentDueDate();

		/// <summary>
		/// Returns the URL to link to for this assignment
		/// (or null if no such URL should be linked to).
		/// </summary>
		string GetAssignmentUrl();

		/// <summary>
		/// Returns child table data for the assignment. This could
		/// differ depending on the type of assignment.
		/// </summary>
		List<object> GetChildTableData();
	}
}
