﻿using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates an assignment report for a single student.
	/// </summary>
	public interface IStudentAssignmentReportGenerator
	{
		/// <summary>
		/// Calculates the scores of all assignment groups for a given student.
		/// </summary>
		StudentAssignmentResults GetStudentAssignmentGroupResults(
			User user,
			Section section,
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> userQuestionSubmissions,
			bool admin);
	}
}
