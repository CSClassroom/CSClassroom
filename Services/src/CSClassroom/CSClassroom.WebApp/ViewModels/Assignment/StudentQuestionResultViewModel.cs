using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

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
		/// The results for each question.
		/// </summary>
		[SubTable(typeof(QuestionSubmissionViewModel))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<QuestionSubmissionViewModel> Submissions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentQuestionResultViewModel(
			StudentQuestionResult questionResult,
			IAssignmentUrlProvider urlProvider,
			ITimeZoneProvider timeZoneProvider)
		{
			var url = urlProvider.GetQuestionUrl(questionResult);
			var name = questionResult.QuestionName;

			QuestionName = url != null 
				? GetLink(url, name, preventWrapping: true)
				: GetColoredText("black", name, bold: false, preventWrapping: true);
			
			ScoreText = GetColoredText
			(
				"black",
				$"{questionResult.Score} / {questionResult.QuestionPoints}",
				bold: false,
				preventWrapping: true
			);

			Status = GetColoredText
			(
				questionResult.Status.GetColor(),
				questionResult.Status.GetText(),
				questionResult.Status.GetBold(),
				preventWrapping: true
			);

			Submissions = questionResult
				.SubmissionResults
				?.Select
				(
					qsr => new QuestionSubmissionViewModel
					(
						qsr,
						urlProvider,
						timeZoneProvider
					)
				).ToList();
		}
	}
}
