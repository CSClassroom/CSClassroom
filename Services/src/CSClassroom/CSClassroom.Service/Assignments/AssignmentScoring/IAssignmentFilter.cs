using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Filters assignments and assignment submissions.
	/// </summary>
	public interface IAssignmentFilter
	{
		/// <summary>
		/// Filters a set of assignments by section, and optionally by group name.
		/// </summary>
		IList<Assignment> FilterAssignments(
			IList<Assignment> assignments,
			Section section,
			string assignmentGroupName = null,
			DateTime? maxDueDate = null);

		/// <summary>
		/// Returns a list of assignment groups for the given section,
		/// ordered by due date.
		/// </summary>
		IList<IGrouping<string, Assignment>> GetAssignmentGroups(
			Section section,
			IList<Assignment> sectionAssignments);

		/// <summary>
		/// Returns all question submissions for the given assignments.
		/// </summary>
		IList<UserQuestionSubmission> FilterSubmissions(
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions,
			User user = null,
			DateTime? snapshotDate = null);
	}
}
