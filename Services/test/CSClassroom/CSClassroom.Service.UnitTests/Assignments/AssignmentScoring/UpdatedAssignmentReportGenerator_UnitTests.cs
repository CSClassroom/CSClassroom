using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using MoreLinq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the UpdatedAssignmentReportGenerator class.
	/// </summary>
	public class UpdatedAssignmentReportGenerator_UnitTests : AssignmentReportGeneratorUnitTestBase
	{
		/// <summary>
		/// The date assignments were last transferred to a gradebook.
		/// </summary>
		private readonly DateTime DateLastTransferred = new DateTime(2017, 1, 1, 0, 0, 0);

		/// <summary>
		/// The date the latest assignments were retrieved from the database (i.e. now).
		/// </summary>
		private readonly DateTime DateRetrieved = new DateTime(2017, 1, 2, 0, 0, 0);

		/// <summary>
		/// The section name.
		/// </summary>
		private readonly string SectionName = "Section Name";

		/// <summary>
		/// The gradebook name.
		/// </summary>
		private readonly string GradebookName = "Gradebook Name";
		
		/// <summary>
		/// Ensures that GetUpdatedAssignmentGroupResults returns a result whose simple
		/// properties are correct.
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentGroupResults_ReturnsCorrectSimpleProperties()
		{
			var section = new Section() { DisplayName = SectionName };
			var users = new List<User>();
			var assignments = new List<Assignment>();
			var submissions = new List<UserQuestionSubmission>();
			var oldSnapshot = new List<SectionAssignmentResults>();
			var newSnapshot = new List<SectionAssignmentResults>();

			var updatedAssignmentReportGenerator = CreateUpdatedAssignmentReportGenerator
			(
				section,
				users,
				assignments,
				submissions,
				oldSnapshot,
				newSnapshot
			);

			var results = updatedAssignmentReportGenerator.GetUpdatedAssignmentGroupResults
			(
				assignments,
				users,
				section,
				GradebookName,
				DateLastTransferred,
				submissions
			);

			Assert.Equal(SectionName, results.SectionName);
			Assert.Equal(GradebookName, results.GradebookName);
			Assert.Equal(DateLastTransferred, results.AssignmentsLastGradedDate);
			Assert.Equal(DateRetrieved, results.ResultsRetrievedDate);
		}

		/// <summary>
		/// Ensures that GetUpdatedAssignmentGroupResults returns no results when
		/// there have been no submissions.
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentGroupResults_NoSubmissionsEver_ReturnsNoResults()
		{
			var section = new Section() { DisplayName = SectionName };
			var users = new List<User>();
			var assignments = new List<Assignment>();
			var submissions = new List<UserQuestionSubmission>();
			var oldSnapshot = new List<SectionAssignmentResults>();
			var newSnapshot = new List<SectionAssignmentResults>();

			var updatedAssignmentReportGenerator = CreateUpdatedAssignmentReportGenerator
			(
				section,
				users,
				assignments,
				submissions,
				oldSnapshot,
				newSnapshot
			);

			var results = updatedAssignmentReportGenerator.GetUpdatedAssignmentGroupResults
			(
				assignments,
				users,
				section,
				GradebookName,
				DateLastTransferred,
				submissions
			);

			Assert.Equal(0, results.AssignmentResults.Count);
		}

		/// <summary>
		/// Ensures that GetUpdatedAssignmentGroupResults returns no results when
		/// there have been no new submissions since the last transfer date.
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentGroupResults_NoNewSubmissions_ReturnsNoResults()
		{
			var section = new Section() { DisplayName = SectionName };
			var users = new List<User>();
			var assignments = new List<Assignment>();
			var submissions = new List<UserQuestionSubmission>();

			var oldSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0)
					)
				),
				CreateSingleAssignmentResults
				(
					"Unit2",
					6.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit2", "Last1", "First1", 6.0)
					)
				)
			);

			var newSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0)
					)
				),
				CreateSingleAssignmentResults
				(
					"Unit2",
					6.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit2", "Last1", "First1", 6.0)
					)
				)
			);

			var updatedAssignmentReportGenerator = CreateUpdatedAssignmentReportGenerator
			(
				section,
				users,
				assignments,
				submissions,
				oldSnapshot,
				newSnapshot
			);

			var results = updatedAssignmentReportGenerator.GetUpdatedAssignmentGroupResults
			(
				assignments,
				users,
				section,
				GradebookName,
				DateLastTransferred,
				submissions
			);

			Assert.Equal(0, results.AssignmentResults.Count);
		}

		/// <summary>
		/// When a student improves on an assignment, ensures that GetUpdatedAssignmentGroupResults 
		/// returns the newly improved score (and no updates for other students).
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentGroupResults_StudentImprovesOnAssignment_ReturnsNewResult()
		{
			var section = new Section() { DisplayName = SectionName };
			var users = new List<User>();
			var assignments = new List<Assignment>();
			var submissions = new List<UserQuestionSubmission>();

			var oldSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0),
						CreateAssignmentGroupResult("Unit1", "Last2", "First2", 4.0)
					)
				)
			);

			var newSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0),
						CreateAssignmentGroupResult("Unit1", "Last2", "First2", 5.0)
					)
				)
			);

			var updatedAssignmentReportGenerator = CreateUpdatedAssignmentReportGenerator
			(
				section,
				users,
				assignments,
				submissions,
				oldSnapshot,
				newSnapshot
			);

			var results = updatedAssignmentReportGenerator.GetUpdatedAssignmentGroupResults
			(
				assignments,
				users,
				section,
				GradebookName,
				DateLastTransferred,
				submissions
			);

			Assert.Equal(1, results.AssignmentResults.Count);

			var sectionAssignmentResults = results.AssignmentResults[0];

			Assert.Equal(SectionName, sectionAssignmentResults.SectionName);
			Assert.Equal("Unit1", sectionAssignmentResults.AssignmentGroupName);
			Assert.Equal(5.0, sectionAssignmentResults.Points);
			Assert.Equal(1, sectionAssignmentResults.AssignmentGroupResults.Count);

			var assignmentGroupResult = sectionAssignmentResults.AssignmentGroupResults[0];

			Assert.Equal("Unit1", assignmentGroupResult.AssignmentGroupName);
			Assert.Equal("Last2", assignmentGroupResult.LastName);
			Assert.Equal("First2", assignmentGroupResult.FirstName);
			Assert.Equal(5.0, assignmentGroupResult.Score);
		}

		/// <summary>
		/// When a student submits an existing assignment for the first time, ensures that 
		/// GetUpdatedAssignmentGroupResults returns the new score (and no updates for other
		/// students).
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentGroupResults_NewStudentSubmittedExistingAssignment_ReturnsNewResult()
		{
			var section = new Section() { DisplayName = SectionName };
			var users = new List<User>();
			var assignments = new List<Assignment>();
			var submissions = new List<UserQuestionSubmission>();

			var oldSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0)
					)
				)
			);

			var newSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0),
						CreateAssignmentGroupResult("Unit1", "Last2", "First2", 4.0)
					)
				)
			);

			var updatedAssignmentReportGenerator = CreateUpdatedAssignmentReportGenerator
			(
				section,
				users,
				assignments,
				submissions,
				oldSnapshot,
				newSnapshot
			);

			var results = updatedAssignmentReportGenerator.GetUpdatedAssignmentGroupResults
			(
				assignments,
				users,
				section,
				GradebookName,
				DateLastTransferred,
				submissions
			);

			Assert.Equal(1, results.AssignmentResults.Count);

			var sectionAssignmentResults = results.AssignmentResults[0];

			Assert.Equal(SectionName, sectionAssignmentResults.SectionName);
			Assert.Equal("Unit1", sectionAssignmentResults.AssignmentGroupName);
			Assert.Equal(5.0, sectionAssignmentResults.Points);
			Assert.Equal(1, sectionAssignmentResults.AssignmentGroupResults.Count);

			var assignmentGroupResult = sectionAssignmentResults.AssignmentGroupResults[0];

			Assert.Equal("Unit1", assignmentGroupResult.AssignmentGroupName);
			Assert.Equal("Last2", assignmentGroupResult.LastName);
			Assert.Equal("First2", assignmentGroupResult.FirstName);
			Assert.Equal(4.0, assignmentGroupResult.Score);
		}

		/// <summary>
		/// When a new assignment is added that has submissions, ensures that 
		/// GetUpdatedAssignmentGroupResults returns the new scores for the
		/// new assignment (and no updates for other assignments).
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentGroupResults_NewAssignment_ReturnsAllResultsForNewAssignment()
		{
			var section = new Section() { DisplayName = SectionName };
			var users = new List<User>();
			var assignments = new List<Assignment>();
			var submissions = new List<UserQuestionSubmission>();

			var oldSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0),
						CreateAssignmentGroupResult("Unit1", "Last2", "First2", 4.0)
					)
				)
			);

			var newSnapshot = Collections.CreateList
			(
				CreateSingleAssignmentResults
				(
					"Unit1",
					5.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit1", "Last1", "First1", 5.0),
						CreateAssignmentGroupResult("Unit1", "Last2", "First2", 4.0)
					)
				),
				CreateSingleAssignmentResults
				(
					"Unit2",
					6.0,
					Collections.CreateList
					(
						CreateAssignmentGroupResult("Unit2", "Last1", "First1", 6.0),
						CreateAssignmentGroupResult("Unit2", "Last2", "First2", 2.0)
					)
				)
			);

			var updatedAssignmentReportGenerator = CreateUpdatedAssignmentReportGenerator
			(
				section,
				users,
				assignments,
				submissions,
				oldSnapshot,
				newSnapshot
			);

			var results = updatedAssignmentReportGenerator.GetUpdatedAssignmentGroupResults
			(
				assignments,
				users,
				section,
				GradebookName,
				DateLastTransferred,
				submissions
			);

			Assert.Equal(1, results.AssignmentResults.Count);

			var sectionAssignmentResults = results.AssignmentResults[0];

			Assert.Equal(SectionName, sectionAssignmentResults.SectionName);
			Assert.Equal("Unit2", sectionAssignmentResults.AssignmentGroupName);
			Assert.Equal(6.0, sectionAssignmentResults.Points);
			Assert.Equal(2, sectionAssignmentResults.AssignmentGroupResults.Count);

			{
				var assignmentGroupResult = sectionAssignmentResults.AssignmentGroupResults[0];

				Assert.Equal("Unit2", assignmentGroupResult.AssignmentGroupName);
				Assert.Equal("Last1", assignmentGroupResult.LastName);
				Assert.Equal("First1", assignmentGroupResult.FirstName);
				Assert.Equal(6.0, assignmentGroupResult.Score);
			}

			{
				var assignmentGroupResult = sectionAssignmentResults.AssignmentGroupResults[1];

				Assert.Equal("Unit2", assignmentGroupResult.AssignmentGroupName);
				Assert.Equal("Last2", assignmentGroupResult.LastName);
				Assert.Equal("First2", assignmentGroupResult.FirstName);
				Assert.Equal(2.0, assignmentGroupResult.Score);
			}
		}

		/// <summary>
		/// Creates a mock section assignment report generator.
		/// </summary>
		private ISnapshotAssignmentReportGenerator CreateMockSnapshotAssignmentReportGenerator(
			Section section,
			IList<User> users,
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions,
			IList<SectionAssignmentResults> oldSectionAssignmentResults,
			IList<SectionAssignmentResults> newSectionAssignmentResults)
		{
			var snapshotAssignmentReportGenerator = new Mock<ISnapshotAssignmentReportGenerator>();

			var snapshots = Collections.CreateList
			(
				new { DateTime = DateLastTransferred, Results = oldSectionAssignmentResults },
				new { DateTime = DateRetrieved, Results = newSectionAssignmentResults }
			);

			foreach (var snapshot in snapshots)
			{
				snapshotAssignmentReportGenerator
					.Setup
					(
						m => m.GetAssignmentGroupResultsSnapshot
						(
							assignments,
							users,
							section,
							submissions,
							snapshot.DateTime
						)
					).Returns(snapshot.Results);
			}

			return snapshotAssignmentReportGenerator.Object;
		}

		/// <summary>
		/// Creates results for a single assignment.
		/// </summary>
		private SectionAssignmentResults CreateSingleAssignmentResults(
			string assignmentGroupName,
			double points,
			IList<AssignmentGroupResult> assignmentGroupResults)
		{
			return new SectionAssignmentResults
			(
				assignmentGroupName,
				SectionName,
				points,
				assignmentGroupResults
			);
		}

		/// <summary>
		/// Creates an updated assignment report generator.
		/// </summary>
		private UpdatedAssignmentReportGenerator CreateUpdatedAssignmentReportGenerator(
			Section section,
			IList<User> users,
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions,
			IList<SectionAssignmentResults> oldSnapshot,
			IList<SectionAssignmentResults> newSnapshot)
		{
			var snapshotAssignmentReportGenerator = CreateMockSnapshotAssignmentReportGenerator
			(
				section,
				users,
				assignments,
				submissions,
				oldSnapshot,
				newSnapshot
			);

			var timeProvider = new Mock<ITimeProvider>();
			timeProvider
				.Setup(m => m.UtcNow)
				.Returns(DateRetrieved);

			return new UpdatedAssignmentReportGenerator
			(
				snapshotAssignmentReportGenerator,
				timeProvider.Object
			);
		}
	}
}
