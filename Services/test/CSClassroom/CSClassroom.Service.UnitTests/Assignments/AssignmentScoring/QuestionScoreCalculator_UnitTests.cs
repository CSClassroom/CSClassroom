using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.AssignmentScoring;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the QuestionScoreCalculator class.
	/// </summary>
	public class QuestionScoreCalculator_UnitTests
	{
		/// <summary>
		/// Verifies that GetSubmissionScore returns the expected result.
		/// </summary>
		[Theory]
		[InlineData("1/1/2017 8:30AM", "1/1/2017 8:30AM", 1.0, true, 1.0)]
		[InlineData("1/1/2017 8:31AM", "1/1/2017 8:30AM", 1.0, true, 0.95)]
		[InlineData("1/1/2017 8:31AM", "1/1/2017 8:30AM", 1.0, false, 1.0)]
		[InlineData("1/1/2017 8:31AM", "1/1/2017 8:30AM", 0.0, true, 0.0)]
		[InlineData("1/5/2017 8:30AM", "1/1/2017 8:30AM", 1.0, true, 0.8)]
		[InlineData("1/10/2017 8:30AM", "1/1/2017 8:30AM", 1.0, true, 0.8)]
		public void GetSubmissionScore_VerifyResult(
			string dateSubmitted,
			string dateDue,
			double score,
			bool withLateness,
			double expectedResult)
		{
			var submission = new UserQuestionSubmission()
			{
				DateSubmitted = DateTime.Parse(dateSubmitted),
				Score = score
			};

			var questionScoreCalculator = new QuestionScoreCalculator();
			var result = questionScoreCalculator.GetSubmissionScore
			(
				submission,
				dateDue != null
					? (DateTime?)DateTime.Parse(dateDue)
					: null,
				1.0 /*questionPoints*/,
				withLateness
			);

			Assert.Equal(expectedResult, result);
		}
	}
}
