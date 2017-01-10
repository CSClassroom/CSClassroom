using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Questions.ServiceResults.Errors;
using CSC.CSClassroom.Service.Questions.QuestionGraders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Moq.Protected;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionGraders
{
	/// <summary>
	/// Unit tests for the CodeQuestionGrader base class.
	/// </summary>
	public class CodeQuestionGrader_UnitTests
	{
		/// <summary>
		/// Ensures we get an error when there is no submission.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NoSubmission_Error()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "" };
			var grader = GetCodeQuestionGrader(question, submission, simulatedResult: null);

			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var noSubmissionErrors = codeQuestionResult.Errors
				.Cast<NoSubmissionError>()
				.ToList();
			
			Assert.Equal(0.0, result.Score);
			Assert.Equal(1, noSubmissionErrors.Count);
		}

		/// <summary>
		/// Ensures we get an error when there is no submission.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_CodeConstraintsViolated_Error()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission()
			{
				Contents = "A B C C C"
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult: null);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var codeConstraintErrors = codeQuestionResult.Errors
				.Cast<CodeConstraintError>()
				.ToList();

			Assert.Equal(0.0, result.Score);
			Assert.Equal(3, codeConstraintErrors.Count);
			Assert.Equal("A", codeConstraintErrors[0].Regex);
			Assert.Equal(1, codeConstraintErrors[0].ActualFrequency);
			Assert.Equal("A-AtLeastTwice", codeConstraintErrors[0].FullErrorText);
			Assert.Equal("B", codeConstraintErrors[1].Regex);
			Assert.Equal(1, codeConstraintErrors[1].ActualFrequency);
			Assert.Equal("B-ExactlyTwice", codeConstraintErrors[1].FullErrorText);
			Assert.Equal("C", codeConstraintErrors[2].Regex);
			Assert.Equal(3, codeConstraintErrors[2].ActualFrequency);
			Assert.Equal("C-AtMostTwice", codeConstraintErrors[2].FullErrorText);
		}

		/// <summary>
		/// Ensures we get an error when the job fails for an unknown reason.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ExecutionFailed_Error()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Error,
				DiagnosticOutput = "Job Failed"
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var diagnosticErrors = codeQuestionResult.Errors
				.Cast<DiagnosticError>()
				.ToList();

			Assert.Equal(0.0, result.Score);
			Assert.Equal(1, diagnosticErrors.Count);
			Assert.Equal("Job Failed", diagnosticErrors.Single().DiagnosticOutput);
		}

		/// <summary>
		/// Ensures we get an error when the job times out.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ExecutionTimedOut_Error()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Timeout
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var timeoutErrors = codeQuestionResult.Errors
				.Cast<TimeoutError>()
				.ToList();

			Assert.Equal(0.0, result.Score);
			Assert.Equal(1, timeoutErrors.Count);
		}

		/// <summary>
		/// Ensures we get an error when the method failed to compile.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassCompilationFailed_Error()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult()
				{
					Success = false,
					Errors = Collections.CreateList
					(
						new CompileError()
						{
							LineNumber = 1,
							Message = "ShortError1",
							FullError = "FullError1"
						},
						new CompileError()
						{
							LineNumber = 2,
							Message = "ShortError2",
							FullError = "FullError2"
						}
					)
				}
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var classCompilationErrors = codeQuestionResult.Errors
				.Cast<ClassCompilationError>()
				.ToList();

			Assert.Equal(0.0, result.Score);
			Assert.Equal(2, classCompilationErrors.Count);
			Assert.Equal(1, classCompilationErrors[0].LineNumber);
			Assert.Equal("ShortError1", classCompilationErrors[0].LineErrorText);
			Assert.Equal("FullError1", classCompilationErrors[0].FullErrorText);
			Assert.Equal(2, classCompilationErrors[1].LineNumber);
			Assert.Equal("ShortError2", classCompilationErrors[1].LineErrorText);
			Assert.Equal("FullError2", classCompilationErrors[1].FullErrorText);
		}

		/// <summary>
		/// Ensures we get an error when the definition does not match.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_DefinitionMismatch_Error()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true }
			};
			var expectedDefinitionErrors = Collections.CreateList<DefinitionError>
			(
				new MethodNameError("expectedMethod", "actualMethod"),
				new MethodReturnTypeError("expectedMethod", "expectedReturnType", "actualReturnType")
			);

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult, expectedDefinitionErrors);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var actualDefinitionErrors = codeQuestionResult.Errors
				.Cast<DefinitionError>()
				.ToList();

			Assert.Equal(0.0, result.Score);
			Assert.Equal(expectedDefinitionErrors, actualDefinitionErrors);
		}

		/// <summary>
		/// Ensures we get an error when the tests failed to compile.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_TestCompilationFailed_Error()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult()
				{
					Success = false,
					Errors = Collections.CreateList
					(
						new CompileError()
						{
							Message = "ShortError1",
							FullError = "FullError1"
						},
						new CompileError()
						{
							Message = "ShortError2",
							FullError = "FullError2"
						}
					)
				}
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testsCompilationErrors = codeQuestionResult.Errors
				.Cast<TestCompilationError>()
				.ToList();

			Assert.Equal(0.0, result.Score);
			Assert.Equal(2, testsCompilationErrors.Count);
			Assert.Null(testsCompilationErrors[0].LineNumber);
			Assert.Null(testsCompilationErrors[0].LineErrorText);
			Assert.True(testsCompilationErrors[0].FullErrorText.Contains("FullError1"));
			Assert.Null(testsCompilationErrors[1].LineNumber);
			Assert.Null(testsCompilationErrors[1].LineErrorText);
			Assert.True(testsCompilationErrors[1].FullErrorText.Contains("FullError2"));
		}

		/// <summary>
		/// Ensures that full credit is received when all tests pass.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_TestsAllPass_FullCredit()
		{
			var question = GetCodeQuestion(numTests: 2);
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = true,
						ReturnValue = "expectedReturnValue",
						Output = "expectedOutput"
					},
					new CodeTestResult()
					{
						Name = "test2",
						Completed = true,
						ReturnValue = "expectedReturnValue",
						Output = "expectedOutput"
					}
				)
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;

			Assert.Equal(1.0, result.Score);
			Assert.Equal(0, codeQuestionResult.Errors.Count);
		}

		/// <summary>
		/// Ensures that partial credit is received when only some tests fail,
		/// when the question allows partial credit.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_SomeTestsPass_PartialCredit()
		{
			var question = GetCodeQuestion(numTests: 2, allowPartialCredit: true);
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = true,
						ReturnValue = "expectedReturnValue",
						Output = "expectedOutput"
					},
					new CodeTestResult()
					{
						Name = "test2",
						Completed = false,
						Exception = "exception"
					}
				)
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;

			Assert.Equal(0.5, result.Score);
			Assert.Equal(0, codeQuestionResult.Errors.Count);
		}

		/// <summary>
		/// Ensures that no credit is received if only some tests pass,
		/// when the question does not allow partial credit.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_SomeTestsPass_NoCredit()
		{
			var question = GetCodeQuestion(numTests: 2, allowPartialCredit: false);
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = true,
						ReturnValue = "expectedReturnValue",
						Output = "expectedOutput"
					},
					new CodeTestResult()
					{
						Name = "test2",
						Completed = false,
						Exception = "exception"
					}
				)
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;

			Assert.Equal(0.0, result.Score);
			Assert.Equal(0, codeQuestionResult.Errors.Count);
		}
		
		/// <summary>
		/// Ensures that results are reported accurately for a passing test.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_TestPasses_AccurateResults()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = true,
						ReturnValue = "expectedReturnValue",
						Output = "expectedOutput"
					}
				)
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testResults = codeQuestionResult.TestResults;

			Assert.Equal(1, testResults.Count);
			Assert.Equal("test1", testResults[0].Description);
			Assert.True(testResults[0].Succeeded);
			Assert.Null(testResults[0].ExceptionText);
			Assert.Equal("expectedReturnValue", testResults[0].ExpectedReturnValue);
			Assert.Equal("expectedReturnValue", testResults[0].ActualReturnValue);
			Assert.Equal("expectedOutput", testResults[0].ExpectedOutput);
			Assert.Equal("expectedOutput", testResults[0].ActualOutput);
		}

		/// <summary>
		/// Ensures that results are reported accurately for failing test,
		/// when the test failed due to incorrect output.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_TestFailsIncorrectOutput_AccurateResults()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = true,
						ReturnValue = "expectedReturnValue",
						Output = "incorrectOutput"
					}
				)
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testResults = codeQuestionResult.TestResults;

			Assert.Equal(1, testResults.Count);
			Assert.Equal("test1", testResults[0].Description);
			Assert.False(testResults[0].Succeeded);
			Assert.Null(testResults[0].ExceptionText);
			Assert.Equal("expectedReturnValue", testResults[0].ExpectedReturnValue);
			Assert.Equal("expectedReturnValue", testResults[0].ActualReturnValue);
			Assert.Equal("expectedOutput", testResults[0].ExpectedOutput);
			Assert.Equal("incorrectOutput", testResults[0].ActualOutput);
		}

		/// <summary>
		/// Ensures that results are reported accurately for failing test,
		/// when the test failed due to an incorrect return value.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_TestFailsIncorrectReturnValue_AccurateResults()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = true,
						ReturnValue = "incorrectReturnValue",
						Output = "expectedOutput"
					}
				)
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testResults = codeQuestionResult.TestResults;

			Assert.Equal(1, testResults.Count);
			Assert.Equal("test1", testResults[0].Description);
			Assert.False(testResults[0].Succeeded);
			Assert.Null(testResults[0].ExceptionText);
			Assert.Equal("expectedReturnValue", testResults[0].ExpectedReturnValue);
			Assert.Equal("incorrectReturnValue", testResults[0].ActualReturnValue);
			Assert.Equal("expectedOutput", testResults[0].ExpectedOutput);
			Assert.Equal("expectedOutput", testResults[0].ActualOutput);
		}

		/// <summary>
		/// Ensures that results are reported accurately for failing test,
		/// when the test failed due to an exception.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_TestFailsException_AccurateResults()
		{
			var question = GetCodeQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "A A B B C C" };
			var simulatedResult = new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = false,
						Exception = "exception"
					}
				)
			};

			var grader = GetCodeQuestionGrader(question, submission, simulatedResult);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testResults = codeQuestionResult.TestResults;

			Assert.Equal(1, testResults.Count);
			Assert.Equal("test1", testResults[0].Description);
			Assert.False(testResults[0].Succeeded);
			Assert.Equal("exception", testResults[0].ExceptionText);
			Assert.Equal("expectedReturnValue", testResults[0].ExpectedReturnValue);
			Assert.Null(testResults[0].ActualReturnValue);
			Assert.Equal("expectedOutput", testResults[0].ExpectedOutput);
			Assert.Null(testResults[0].ActualOutput);
		}

		/// <summary>
		/// Returns a code question grader. For a given question, the grader 
		/// returns a given simulated result for a given submission.
		/// </summary>
		public CodeQuestionGrader<CodeQuestion> GetCodeQuestionGrader(
			CodeQuestion question,
			CodeQuestionSubmission submission,
			CodeJobResult simulatedResult,
			IList<DefinitionError> definitionErrors = null)
		{
			var grader = new Mock<CodeQuestionGrader<CodeQuestion>>
			(
				question,
				null /*codeRunnerService*/
			);

			grader.CallBase = true;

			grader.Protected()
				.Setup<Task<CodeJobResult>>
				(
					"ExecuteJobAsync", 
					ItExpr.Is<CodeQuestionSubmission>(s => s == submission)
				).Returns(Task.FromResult(simulatedResult));

			grader.Protected()
				.Setup<string>("GetTestDescription", ItExpr.IsAny<CodeQuestionTest>())
				.Returns<CodeQuestionTest>(test => test.Name);

			if (definitionErrors != null)
			{
				grader.Protected()
					.Setup<IEnumerable<CodeQuestionError>>
					(
						"GetDefinitionErrors",
						ItExpr.Is<CodeQuestionSubmission>(s => s == submission),
						ItExpr.Is<CodeJobResult>(r => r == simulatedResult)
					).Returns(definitionErrors);
			}

			return grader.Object;
		}

		/// <summary>
		/// Returns a new code question.
		/// </summary>
		public CodeQuestion GetCodeQuestion(int numTests = 1, bool allowPartialCredit = false)
		{
			return new MethodQuestion()
			{
				AllowPartialCredit = allowPartialCredit,
				ImportedClasses = Collections.CreateList
				(
					new ImportedClass() { ClassName = "package.classToImport" }
				),
				CodeConstraints = Collections.CreateList
				(
					new CodeConstraint()
					{
						Regex = "A",
						Type = CodeConstraintType.AtLeast,
						Frequency = 2,
						ErrorMessage = "A-AtLeastTwice"
					},
					new CodeConstraint()
					{
						Regex = "B",
						Type = CodeConstraintType.Exactly,
						Frequency = 2,
						ErrorMessage = "B-ExactlyTwice"
					},
					new CodeConstraint()
					{
						Regex = "C",
						Type = CodeConstraintType.AtMost,
						Frequency = 2,
						ErrorMessage = "C-AtMostTwice"
					}
				),
				Tests = new int[numTests]
					.Select
					(
						(value, index) => new MethodQuestionTest()
						{
							Name = $"test{index + 1}",
							Order = index + 1,
							ExpectedReturnValue = "expectedReturnValue",
							ExpectedOutput = "expectedOutput"
						}
					).ToList()
			};
		}
	}
}
