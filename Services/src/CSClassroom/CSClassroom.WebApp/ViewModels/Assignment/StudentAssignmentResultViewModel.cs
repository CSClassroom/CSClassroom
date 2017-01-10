using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// The result for a single student's assignment.
	/// </summary>
	public class StudentAssignmentResultViewModel
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
		/// The results for each question.
		/// </summary>
		[JsonProperty(PropertyName = "childTableData")]
		public List<StudentQuestionResultViewModel> QuestionResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentResultViewModel(
			SectionAssignmentResult assignmentResult)
		{
			LastName = assignmentResult.LastName;
			FirstName = assignmentResult.FirstName;
			Score = assignmentResult.Score;
			QuestionResults = assignmentResult.QuestionResults.Select
			(
				questionResult => new StudentQuestionResultViewModel
				(
					questionResult, 
					getQuestionUrl: null
				)
			).ToList();
		}
	}
}
