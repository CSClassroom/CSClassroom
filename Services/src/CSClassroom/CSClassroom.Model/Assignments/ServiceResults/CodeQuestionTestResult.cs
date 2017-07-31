namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// A test result for a code question.
	/// </summary>
	public class CodeQuestionTestResult
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public CodeQuestionTestResult(
			string description,
			string expectedOutput,
			string actualOutput,
			string expectedReturnValue,
			string actualReturnValue,
			string exceptionText,
			string visualizeUrl,
			bool succeeded)
		{
			Description = description;
			ExpectedOutput = expectedOutput;
			ActualOutput = actualOutput;
			ExpectedReturnValue = expectedReturnValue;
			ActualReturnValue = actualReturnValue;
			ExceptionText = exceptionText;
			VisualizeUrl = visualizeUrl;
			Succeeded = succeeded;
		}

		/// <summary>
		/// A description of the test.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// The expected output.
		/// </summary>
		public string ExpectedOutput { get; }

		/// <summary>
		/// The actual output.
		/// </summary>
		public string ActualOutput { get; }

		/// <summary>
		/// The expected return value.
		/// </summary>
		public string ExpectedReturnValue { get; }

		/// <summary>
		/// The actual return value.
		/// </summary>
		public string ActualReturnValue { get; }

		/// <summary>
		/// The text of the exception thrown when running the test, if any.
		/// </summary>
		public string ExceptionText { get; }

		/// <summary>
		/// The URL to visualize the execution of this test.
		/// </summary>
		public string VisualizeUrl { get; }

		/// <summary>
		/// Whether or not the test succeeded.
		/// </summary>
		public bool Succeeded { get; }
	}
}
