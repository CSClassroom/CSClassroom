using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
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
		public List<AssignmentResultViewModel> AssignmentResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentsViewModel(
			StudentAssignmentResults results,
			Func<int, string> getQuestionUrl,
			ITimeZoneProvider timeZoneProvider)
		{
			LastName = results.LastName;
			FirstName = results.FirstName;
			SectionName = results.SectionName;
			AssignmentResults = results.AssignmentResults.Select
			(
				result => new AssignmentResultViewModel
				(
					result,
					getQuestionUrl,
					timeZoneProvider
				)
			).ToList();
		}
	}
}
