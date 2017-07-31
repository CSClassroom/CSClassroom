using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates an assignment report with updated assignment results.
	/// </summary>
	public interface IUpdatedAssignmentReportGenerator
	{
		/// <summary>
		/// Calculates the scores for all assignment groups updated since the last time 
		/// assignments were marked as graded, for a given section.
		/// </summary>
		UpdatedSectionAssignmentResults GetUpdatedAssignmentGroupResults(
			IList<Assignment> assignments,
			IList<User> users,
			Section section,
			string gradebookName,
			DateTime lastTransferDate,
			IList<UserQuestionSubmission> userQuestionSubmissions);
	}
}
