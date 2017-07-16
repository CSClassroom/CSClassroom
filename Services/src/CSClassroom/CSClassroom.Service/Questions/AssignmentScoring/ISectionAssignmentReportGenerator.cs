using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
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
