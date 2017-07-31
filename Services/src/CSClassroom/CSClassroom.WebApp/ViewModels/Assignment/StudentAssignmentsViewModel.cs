using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// A report containing all of a student's assignments.
	/// </summary>
	public class StudentAssignmentsViewModel
	{
		/// <summary>
		/// The student's last name.
		/// </summary>
		public string LastName { get; }

		/// <summary>
		/// The student's first name.
		/// </summary>
		public string FirstName { get; }

		/// <summary>
		/// The section name.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// The results for each student
		/// </summary>
		public List<AssignmentGroupResultViewModel> AssignmentGroupResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentsViewModel(
			StudentAssignmentResults results,
			IAssignmentDisplayProviderFactory displayProviderFactory)
		{
			LastName = results.LastName;
			FirstName = results.FirstName;
			SectionName = results.SectionName;
			AssignmentGroupResults = results.AssignmentGroupResults.Select
			(
				result => new AssignmentGroupResultViewModel
				(
					result,
					displayProviderFactory
				)
			).ToList();
		}
	}
}
