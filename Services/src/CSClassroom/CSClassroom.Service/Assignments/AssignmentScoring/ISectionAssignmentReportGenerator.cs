using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates an assignment report for all students in a section.
	/// </summary>
	public interface ISectionAssignmentReportGenerator
	{
		/// <summary>
		/// Calculates the scores for a single assignment group in a single section,
		/// for all students.
		/// </summary>
		SectionAssignmentResults GetSectionAssignmentGroupResults(
			string assignmentGroupName,
			IList<Assignment> assignments,
			Section section,
			IList<User> users,
			IList<UserQuestionSubmission> userQuestionSubmissions);
	}
}
