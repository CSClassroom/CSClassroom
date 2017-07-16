using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
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
