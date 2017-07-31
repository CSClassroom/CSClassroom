using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	public class AssignmentFilter : IAssignmentFilter
	{
		/// <summary>
		/// Filters a set of assignments by section, and optionally by group name.
		/// </summary>
		public IList<Assignment> FilterAssignments(
			IList<Assignment> assignments,
			Section section,
			string assignmentGroupName = null)
		{
			return assignments
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
