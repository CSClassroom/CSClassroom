using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.Extensions;
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
	public class SnapshotAssignmentReportGenerator : ISnapshotAssignmentReportGenerator
	{
		/// <summary>
		/// The assignment filter.
		/// </summary>
		private readonly IAssignmentFilter _assignmentFilter;

		/// <summary>
		/// The section assignment report generator.
		/// </summary>
		private readonly ISectionAssignmentReportGenerator _sectionAssignmentReportGenerator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SnapshotAssignmentReportGenerator(
			IAssignmentFilter assignmentFilter,
			ISectionAssignmentReportGenerator sectionAssignmentReportGenerator)
		{
			_assignmentFilter = assignmentFilter;
			_sectionAssignmentReportGenerator = sectionAssignmentReportGenerator;
		}

		/// <summary>
		/// Returns the assignment results for the given set of submissions.
		/// </summary>
		public IList<SectionAssignmentResults> GetAssignmentGroupResultsSnapshot(
			IList<Assignment> assignments,
			IList<User> users,
			Section section,
			IList<UserQuestionSubmission> submissions,
			DateTime snapshotDate)
		{
			var sectionAssignments = _assignmentFilter.FilterAssignments
			(
				assignments,
				section
			);

			var assignmentGroups = _assignmentFilter.GetAssignmentGroups
			(
				section,
				sectionAssignments
			);

			var sectionAssignmentSubmissions = _assignmentFilter.FilterSubmissions
			(
				sectionAssignments,
				submissions,
				null /*user*/,
				snapshotDate
			);

			var submissionsByGroup = sectionAssignmentSubmissions.GroupBy
			(
				submission => submission.UserQuestionData
					.AssignmentQuestion
					.Assignment
					.GroupName
			).ToDictionary(g => g.Key, g => g.ToList());

			return assignmentGroups.Select
			(
				assignmentGroup => _sectionAssignmentReportGenerator
					.GetSectionAssignmentGroupResults
					(
						assignmentGroup.Key,
						assignmentGroup.ToList(),
						section,
						users,
						submissionsByGroup.GetValueOrNew(assignmentGroup.Key)
					)
			).ToList();
		}
	}
}
