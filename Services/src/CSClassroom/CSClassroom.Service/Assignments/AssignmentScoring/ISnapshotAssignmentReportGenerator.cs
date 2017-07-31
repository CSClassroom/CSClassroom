using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates group results for all users/assignment groups in a given
	/// section, up to a given snapshot date.
	/// </summary>
	public interface ISnapshotAssignmentReportGenerator
	{
		/// <summary>
		/// Returns the assignment results for the given set of submissions.
		/// </summary>
		IList<SectionAssignmentResults> GetAssignmentGroupResultsSnapshot(
			IList<Assignment> assignments,
			IList<User> users,
			Section section,
			IList<UserQuestionSubmission> submissions,
			DateTime snapshotDate);
	}
}
