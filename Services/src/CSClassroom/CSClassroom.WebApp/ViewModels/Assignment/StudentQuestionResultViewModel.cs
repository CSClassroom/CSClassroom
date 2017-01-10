using System;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// The result for a single problem, for a single student.
	/// </summary>
	public class StudentQuestionResultViewModel : TableEntry
	{
		/// <summary>
		/// The name of the question.
		/// </summary>
		[TableColumn("Question")]
		public string QuestionName { get; }

		/// <summary>
		/// The score for the question.
		/// </summary>
		[TableColumn("Score")]
		public string ScoreText { get; }

		/// <summary>
		/// The status for the question.
		/// </summary>
		[TableColumn("Status")]
		public string Status { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentQuestionResultViewModel(
			StudentQuestionResult result,
			Func<int, string> getQuestionUrl)
		{
			QuestionName = getQuestionUrl != null 
				? GetLink
					(
						getQuestionUrl(result.QuestionId), 
						result.QuestionName, 
						preventWrapping: true
					)
				: GetColoredText
					(
						"black", 
						result.QuestionName, 
						bold: false,
						preventWrapping: true
					);
			
			ScoreText = GetColoredText
			(
				"black",
				$"{result.Score} / {result.QuestionPoints}",
				bold: false,
				preventWrapping: true
			);

			Status = GetColoredText
			(
				result.Status.GetColor(), 
				result.Status.GetText(), 
				result.Status.GetBold(),
				preventWrapping: true
			); 
		}
	}
}
