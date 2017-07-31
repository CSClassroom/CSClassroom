using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates results for a single user/assignment group.
	/// </summary>
	public class AssignmentGroupResultGenerator : IAssignmentGroupResultGenerator
	{
		/// <summary>
		/// Generates assignment results.
		/// </summary>
		private readonly IAssignmentResultGenerator _assignmentResultGenerator;

		/// <summary>
		/// Filters assignments to the assignments in a given group.
		/// </summary>
		private readonly IAssignmentFilter _assignmentFilter;

		/// <summary>
		/// Calculates the score and status for an assignment group.
		/// </summary>
		private readonly IAssignmentGroupScoreCalculator _assignmentGroupScoreCalculator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentGroupResultGenerator(
			IAssignmentResultGenerator assignmentResultGenerator,
			IAssignmentFilter assignmentFilter,
			IAssignmentGroupScoreCalculator assignmentGroupScoreCalculator)
		{
			_assignmentResultGenerator = assignmentResultGenerator;
			_assignmentFilter = assignmentFilter;
			_assignmentGroupScoreCalculator = assignmentGroupScoreCalculator;
		}

		/// <summary>
		/// Returns an assignment group result, for a given user and assignment group.
		/// </summary>
		public AssignmentGroupResult GetAssignmentGroupResult(
			string assignmentGroupName,
			IList<Assignment> assignments,
			Section section,
			User user,
			IList<UserQuestionSubmission> submissions,
			bool admin)
		{
			var sectionAssignmentsInGroup = _assignmentFilter.FilterAssignments
			(
				assignments,
				section,
				assignmentGroupName
			);

			var assignmentGroupSubmissionsForUser = _assignmentFilter.FilterSubmissions
			(
				sectionAssignmentsInGroup,
				submissions,
				user
			);

			var assignmentResults = sectionAssignmentsInGroup.Select
			(
				assignment => _assignmentResultGenerator.CreateAssignmentResult
				(
					section,
					assignment,
					user,
					admin,
					assignmentGroupSubmissionsForUser
				)
			).ToList();

			return new AssignmentGroupResult
			(
				assignmentGroupName,
				user.LastName,
				user.FirstName,
				_assignmentGroupScoreCalculator.GetAssignmentGroupScore
				(
					assignmentResults, 
					roundDigits: 1
				),
				_assignmentGroupScoreCalculator.GetAssignmentGroupTotalPoints
				(
					sectionAssignmentsInGroup, 
					roundDigits: 1
				),
				assignmentResults,
				_assignmentGroupScoreCalculator.GetAssignmentGroupStatus
				(
					assignmentResults
				)
			);
		}
	}
}
