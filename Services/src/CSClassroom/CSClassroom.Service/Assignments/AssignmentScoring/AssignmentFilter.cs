using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	public class AssignmentFilter : IAssignmentFilter
	{
		/// <summary>
		/// Filters a set of assignments by section, and optionally by group name.
		/// </summary>
		public IList<Assignment> FilterAssignments(
			IList<Assignment> assignments,
			Section section,
			string assignmentGroupName = null,
			DateTime? maxDueDate = null)
		{
			var filteredAssignments = assignments
				.Where(a => assignmentGroupName == null || a.GroupName == assignmentGroupName)
				.Where
				(
					a => section == null
					     || (a.DueDates?.Any(d => d.Section == section) ?? false)
				)
				.OrderBy
				(
					a => a.GetDueDate(section) ?? DateTime.MaxValue
				)
				.ThenBy(a => a.Name, new NaturalComparer())
				.ToList();

			if (maxDueDate != null)
			{
				var assignmentGroupDueDates = assignments
					.GroupBy(a => a.GroupName)
					.ToDictionary
					(
						g => g.Key,
						g => g.Max(a => a.DueDates.Single(d => d.Section == section).DueDate)
					);

				filteredAssignments = filteredAssignments
					.Where(a => assignmentGroupDueDates[a.GroupName] <= maxDueDate)
					.ToList();
			}

			return filteredAssignments;
		}

		/// <summary>
		/// Returns a list of assignment groups for the given section,
		/// ordered by due date.
		/// </summary>
		public IList<IGrouping<string, Assignment>> GetAssignmentGroups(
			Section section,
			IList<Assignment> sectionAssignments)
		{
			return sectionAssignments
				.GroupBy(assignment => assignment.GroupName)
				.OrderBy
				(
					assignmentGroup => assignmentGroup.Max
					(
						a => a.GetDueDate(section) ?? DateTime.MaxValue
					)
				)
				.ThenBy
				(
					assignmentGroup => assignmentGroup.Key,
					new NaturalComparer()
				).ToList();
		}

		/// <summary>
		/// Returns all question submissions for the given assignments.
		/// </summary>
		public IList<UserQuestionSubmission> FilterSubmissions(
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions,
			User user = null,
			DateTime? snapshotDate = null)
		{
			var assignmentIds = new HashSet<int>
			(
				assignments
					.Select(a => a.Id)
					.ToList()
			);

			return submissions
				.Where(s => user == null || s.UserQuestionData.UserId == user.Id)
				.Where(s => snapshotDate == null || s.DateSubmitted <= snapshotDate)
				.Where
				(
					s => assignmentIds.Contains
					(
						s.UserQuestionData.AssignmentQuestion.AssignmentId
					)
				).ToList();
		}
	}
}
