using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// The base class for assignment report generator unit tests.
	/// </summary>
	public class AssignmentReportGeneratorUnitTestBase
	{
		/// <summary>
		/// Represents a call to FilterAssignments.
		/// </summary>
		protected class FilterAssignmentsCall
		{
			/// <summary>
			/// The assignments being filtered.
			/// </summary>
			public IList<Assignment> Assignments { get; }

			/// <summary>
			/// The resulting filtered assignments.
			/// </summary>
			public IList<Assignment> FilteredAssignments { get; }

			/// <summary>
			/// The assignment group name to filter by (if any).
			/// </summary>
			public string AssignmentGroupName { get; }

			/// <summary>
			/// Constructor.
			/// </summary>
			public FilterAssignmentsCall(
				IList<Assignment> assignments,
				IList<Assignment> filteredAssignments,
				string assignmentGroupName = null)
			{
				Assignments = assignments;
				FilteredAssignments = filteredAssignments;
				AssignmentGroupName = assignmentGroupName;
			}
		}
		
		/// <summary>
		/// Represents a call to FilterSubmissions.
		/// </summary>
		protected class FilterSubmissionsCall
		{
			/// <summary>
			/// The submissions being filtered.
			/// </summary>
			public IList<UserQuestionSubmission> Submissions { get; }

			/// <summary>
			/// The resulting filtered submissions.
			/// </summary>
			public IList<UserQuestionSubmission> FilteredSubmissions { get; }

			/// <summary>
			/// The user to filter to (if any).
			/// </summary>
			public User User { get; }

			/// <summary>
			/// The snapshot date to filter to (if any).
			/// </summary>
			public DateTime? SnapshotDate { get; }

			/// <summary>
			/// Constructor.
			/// </summary>
			public FilterSubmissionsCall(
				IList<UserQuestionSubmission> submissions,
				IList<UserQuestionSubmission> filteredSubmissions,
				User user = null,
				DateTime? snapshotDate = null)
			{
				Submissions = submissions;
				FilteredSubmissions = filteredSubmissions;
				User = user;
				SnapshotDate = snapshotDate;
			}
		}

		/// <summary>
		/// Represents a call to GetAssignmentGroupResult.
		/// </summary>
		protected class GetAssignmentGroupResultCall
		{
			/// <summary>
			/// The assignment group.
			/// </summary>
			public IGrouping<string, Assignment> AssignmentGroup { get; }

			/// <summary>
			/// The user.
			/// </summary>
			public User User { get; }

			/// <summary>
			/// The result.
			/// </summary>
			public AssignmentGroupResult Result { get; }

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="assignmentGroup"></param>
			/// <param name="user"></param>
			/// <param name="result"></param>
			public GetAssignmentGroupResultCall(
				IGrouping<string, Assignment> assignmentGroup,
				User user,
				AssignmentGroupResult result)
			{
				AssignmentGroup = assignmentGroup;
				User = user;
				Result = result;
			}
		}

		/// <summary>
		/// Represents a call to GetSectionAssignmentGroupResults.
		/// </summary>
		protected class GetSectionAssignmentGroupResultsCall
		{
			/// <summary>
			/// The assignment group.
			/// </summary>
			public IGrouping<string, Assignment> AssignmentGroup { get; }

			/// <summary>
			/// The result.
			/// </summary>
			public SectionAssignmentResults Results { get; }

			/// <summary>
			/// Constructor.
			/// </summary>
			public GetSectionAssignmentGroupResultsCall(
				IGrouping<string, Assignment> assignmentGroup,
				SectionAssignmentResults results)
			{
				AssignmentGroup = assignmentGroup;
				Results = results;
			}
		}

		/// <summary>
		/// Returns a mock assignment filter.
		/// </summary>
		protected IAssignmentFilter CreateMockAssignmentFilter(
			Section section,
			FilterAssignmentsCall filterAssignmentsCall,
			FilterSubmissionsCall filterSubmissionsCall,
			IList<IGrouping<string, Assignment>> assignmentGroups)
		{
			var assignmentFilter = new Mock<IAssignmentFilter>();

			assignmentFilter
				.Setup
				(
					m => m.FilterAssignments
					(
						filterAssignmentsCall.Assignments, 
						section, 
						filterAssignmentsCall.AssignmentGroupName
					)
				).Returns(filterAssignmentsCall.FilteredAssignments);

			assignmentFilter
				.Setup
				(
					m => m.FilterSubmissions
					(
						filterAssignmentsCall.FilteredAssignments,
						filterSubmissionsCall.Submissions,
						filterSubmissionsCall.User,
						filterSubmissionsCall.SnapshotDate
					)
				).Returns(filterSubmissionsCall.FilteredSubmissions);

			assignmentFilter
				.Setup
				(
					m => m.GetAssignmentGroups
					(
						section,
						filterAssignmentsCall.FilteredAssignments
					)
				).Returns(assignmentGroups);

			return assignmentFilter.Object;
		}

		/// <summary>
		/// Creates a mock assignment group result generator.
		/// </summary>
		protected IAssignmentGroupResultGenerator CreateMockAssignmentGroupResultGenerator(
			Section section,
			IList<GetAssignmentGroupResultCall> getAssignmentGroupResultCalls,
			IList<UserQuestionSubmission> submissions,
			bool admin)
		{
			var assignmentGroupResultGenerator = new Mock<IAssignmentGroupResultGenerator>();

			foreach (var getAssignmentGroupResultCall in getAssignmentGroupResultCalls)
			{
				assignmentGroupResultGenerator
					.Setup
					(
						m => m.GetAssignmentGroupResult
						(
							getAssignmentGroupResultCall.AssignmentGroup.Key,
							It.Is<IList<Assignment>>
							(
								seq => seq.SequenceEqual(getAssignmentGroupResultCall.AssignmentGroup)
							),
							section,
							getAssignmentGroupResultCall.User,
							It.Is<IList<UserQuestionSubmission>>
							(
								seq => seq.SequenceEqual
								(
									submissions.Where
									(
										s => s.UserQuestionData.User == getAssignmentGroupResultCall.User
									)
								)
							),
							admin
						)
					).Returns(getAssignmentGroupResultCall.Result);
			}

			return assignmentGroupResultGenerator.Object;
		}

		/// <summary>
		/// Creates an assignment result for the given assignment.
		/// </summary>
		protected AssignmentGroupResult CreateAssignmentGroupResult(
			IGrouping<string, Assignment> assignmentGroup,
			User user)
		{
			return CreateAssignmentGroupResult
			(
				assignmentGroup.Key,
				user.LastName,
				user.FirstName,
				score: 0.0
			);
		}

		/// <summary>
		/// Creates an assignment result for the given assignment.
		/// </summary>
		protected AssignmentGroupResult CreateAssignmentGroupResult(
			string assignmentGroupName,
			string lastName,
			string firstName,
			double score)
		{
			return new AssignmentGroupResult
			(
				assignmentGroupName,
				lastName,
				firstName,
				score,
				totalPoints: 0.0,
				assignmentResults: null,
				status: null
			);
		}

		/// <summary>
		/// Creates a SectionAssignmentResults object.
		/// </summary>
		protected SectionAssignmentResults CreateSectionAssignmentResults(
			IGrouping<string, Assignment> assignmentGroup,
			IList<AssignmentGroupResult> assignmentGroupResults = null)
		{
			return new SectionAssignmentResults
			(
				assignmentGroup.Key,
				null /*sectionName*/,
				0.0 /*points*/,
				assignmentGroupResults: assignmentGroupResults
			);
		}

		/// <summary>
		/// Creates a new question submission.
		/// </summary>
		protected UserQuestionSubmission CreateSubmission(
			int id, 
			User user, 
			Assignment assignment = null)
		{
			return new UserQuestionSubmission()
			{
				Id = id,
				UserQuestionData = new UserQuestionData()
				{
					User = user,
					UserId = user.Id,
					AssignmentQuestion = new AssignmentQuestion()
					{
						Assignment = assignment
					}
				},
			};
		}
	}
}
