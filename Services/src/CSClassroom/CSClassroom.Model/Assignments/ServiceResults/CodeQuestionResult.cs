using System.Collections.Generic;
using CSC.CSClassroom.Model.Questions.ServiceResults.Errors;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// A result of a code question submission.
	/// </summary>
	public class CodeQuestionResult : QuestionResult
	{
		public CodeQuestionResult(
			List<CodeQuestionError> errors,
			List<CodeQuestionTestResult> testResults)
		{
			Errors = errors;
			TestResults = testResults;
		}
		
		/// <summary>
		/// Any errors associated with the question.
		/// </summary>
		public List<CodeQuestionError> Errors { get; }

		/// <summary>
		/// The test results.
		/// </summary>
		public List<CodeQuestionTestResult> TestResults { get; }
	}
}
