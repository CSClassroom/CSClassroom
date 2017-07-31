using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// The result for a single student's assignment.
	/// </summary>
	public class StudentAssignmentResultViewModel : TableEntry
	{
		/// <summary>
		/// The student's last name.
		/// </summary>
		[TableColumn("Last Name")]
		public string LastName { get; }

		/// <summary>
		/// The student's first name.
		/// </summary>
		[TableColumn("First Name")]
		public string FirstName { get; }

		/// <summary>
		/// The student's score for the assignment.
		/// </summary>
		[TableColumn("Score")]
		public double Score { get; }

		/// <summary>
		/// The assignment status.
		/// </summary>
		[TableColumn("Status")]
		public string Status { get; }

		/// <summary>
		/// The results for each question.
		/// </summary>
		[SubTable(typeof(StudentQuestionResultViewModel), typeof(AssignmentResultViewModel), typeof(AssignmentSubmissionViewModel))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<object> ChildTableData { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentResultViewModel(
			AssignmentGroupResult assignmentGroupResult,
			IAssignmentDisplayProviderFactory displayProviderFactory)
		{
			LastName = assignmentGroupResult.LastName;
			FirstName = assignmentGroupResult.FirstName;
			Score = assignmentGroupResult.Score;

			Status = GetColoredText
			(
				assignmentGroupResult.Status.GetColor(),
				assignmentGroupResult.Status.GetText(),
				assignmentGroupResult.Status.GetBold(),
				preventWrapping: true
			);

			if (assignmentGroupResult.AssignmentResults.Count == 1)
			{
				var assignmentResult = assignmentGroupResult.AssignmentResults[0];
				var displayProvider = displayProviderFactory.CreateDisplayProvider
				(
					assignmentResult
				);

				ChildTableData = displayProvider.GetChildTableData();
			}
			else
			{
				ChildTableData = assignmentGroupResult.AssignmentResults
					.Select
					(
						assignmentResult => new AssignmentResultViewModel
						(
							assignmentResult,
							displayProviderFactory.CreateDisplayProvider(assignmentResult)
						)
					).Cast<object>().ToList();
			}
		}
	}
}
