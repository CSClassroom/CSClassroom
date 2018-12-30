using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.Common.Infrastructure.Extensions;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Assignments.ServiceResults.Errors;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;

namespace CSC.CSClassroom.Service.Assignments.QuestionGraders
{
	/// <summary>
	/// Grades a code question.
	/// </summary>
	public abstract class CodeQuestionGrader<TQuestion> : QuestionGrader<TQuestion> where TQuestion : CodeQuestion
	{
		/// <summary>
		/// The code runner service.
		/// </summary>
		protected ICodeRunnerService CodeRunnerService { get; }

		/// <summary>
		/// The URL for the visualizer.
		/// </summary>
		private const string c_visualizerBaseUrl = "http://cscircles.cemc.uwaterloo.ca/java_visualize/";

		/// <summary>
		/// Constructor.
		/// </summary>
		protected CodeQuestionGrader(TQuestion question, ICodeRunnerService codeRunnerService) 
			: base(question)
		{
			CodeRunnerService = codeRunnerService;
		}

		/// <summary>
		/// Executes a code job to grade the question.
		/// </summary>
		protected abstract Task<CodeJobResult> ExecuteJobAsync(CodeQuestionSubmission submission);

		/// <summary>
		/// Returns any errors with the submission's definition (classes/methods/code).
		/// </summary>
		protected abstract IEnumerable<CodeQuestionError> GetDefinitionErrors(
			CodeQuestionSubmission submission, 
			CodeJobResult jobResult);

		/// <summary>
		/// Returns the description of a code test.
		/// </summary>
		protected abstract string GetTestDescription(CodeQuestionTest test);

		/// <summary>
		/// Returns the command line arguments for the given test.
		/// </summary>
		protected abstract string[] GetVisualizerCommandLineArguments(CodeQuestionTest test);

		/// <summary>
		/// Adds a class to a java file to run in the visualizer, for the given test.
		/// </summary>
		protected abstract void AddVisualizerClass(
			JavaFileBuilder builder, 
			CodeQuestionTest test, 
			CodeQuestionSubmission submission);

		/// <summary>
		/// Allows for returning a more friendly error, for compiler errors that
		/// aren't as friendly.
		/// </summary>
		protected virtual CodeQuestionError GetFriendlyError(
			ClassCompilationError compilationError)
		{
			return compilationError;
		}

		/// <summary>
		/// Returns the test name to use in the code job 
		/// </summary>
		protected string GetCodeTestName(CodeQuestionTest test)
		{
			return $"test{test.Order}";
		}

		/// <summary>
		/// Returns the given type without generic type parameters.
		/// </summary>
		protected string StripGenericsFromType(string type) 
		{
			if (type == null) 
			{
				return null;
			}

			int genericIndex = type.IndexOf("<");
			if (genericIndex != -1) 
			{
				return type.Substring(0, genericIndex);
			}
			else 
			{
				return type;
			}
		}

		/// <summary>
		/// Grades the question submission.
		/// </summary>
		public override sealed async Task<ScoredQuestionResult> GradeSubmissionAsync(QuestionSubmission submission)
		{
			var codeQuestionSubmission = submission as CodeQuestionSubmission;
			if (codeQuestionSubmission == null)
				throw new ArgumentException("Invalid submission type", nameof(submission));

			if (string.IsNullOrEmpty(codeQuestionSubmission.Contents))
			{
				return CreateNoSubmissionResult();
			}

			var preJobExecutionErrors = GetConstraintErrors(codeQuestionSubmission).ToList();
			if (preJobExecutionErrors.Count > 0)
			{
				return new ScoredQuestionResult
				(
					new CodeQuestionResult(preJobExecutionErrors, testResults: null),
					0.0 /*score*/
				);
			}

			var jobResult = await ExecuteJobAsync(codeQuestionSubmission);

			var postJobExecutionErrors = GetPostJobExecutionErrors
			(
				codeQuestionSubmission, 
				jobResult
			).ToList();

			var testResults = !postJobExecutionErrors.Any() 
				? GetTestResults(jobResult, codeQuestionSubmission).ToList() 
				: null;

			return new ScoredQuestionResult
			(
				new CodeQuestionResult(postJobExecutionErrors, testResults),
				Question.AllowPartialCredit && testResults != null
					? (testResults.Count(t => t.Succeeded) * 1.0)/testResults.Count
					: testResults?.All(t => t.Succeeded) ?? false ? 1.0 : 0.0
			);
		}

		/// <summary>
		/// Creates a no-submission result.
		/// </summary>
		private static ScoredQuestionResult CreateNoSubmissionResult()
		{
			return new ScoredQuestionResult
			(
				new CodeQuestionResult
				(
					new List<CodeQuestionError>() {new NoSubmissionError()},
					testResults: null
				),
				score: 0.0
			);
		}

		/// <summary>
		/// Returns a list of errors for the job.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetPostJobExecutionErrors(
			CodeQuestionSubmission submission, 
			CodeJobResult jobResult)
		{
			return GetFirstNonEmptyErrorSet
			(
				submission,
				jobResult,
				new ComputeErrorSet[]
				{
					GetJobExecutionErrors,
					GetClassCompilationErrors,
					GetDefinitionErrors,
					GetTestCompilationErrors
				}
			);
		}

		/// <summary>
		/// A function that computes a set of errors.
		/// </summary>
		private delegate IEnumerable<CodeQuestionError> ComputeErrorSet(
			CodeQuestionSubmission submission, 
			CodeJobResult jobResult);

		/// <summary>
		/// Returns the first non-empty set computed by the given compute functions.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetFirstNonEmptyErrorSet(
			CodeQuestionSubmission submission,
			CodeJobResult jobResult,
			ComputeErrorSet[] computeErrorSets)
		{
			foreach (var computeErrorSet in computeErrorSets)
			{
				var errors = computeErrorSet(submission, jobResult).ToList();
				if (errors.Any())
					return errors.ToList();
			}

			return Enumerable.Empty<CodeQuestionError>();
		}

		/// <summary>
		/// Returns any errors that prevented the job from executing.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetJobExecutionErrors(
			CodeQuestionSubmission submission, 
			CodeJobResult jobResult)
		{
			switch (jobResult.Status)
			{
				case CodeJobStatus.Timeout:
					yield return new TimeoutError();
					break;

				case CodeJobStatus.Error:
					yield return new DiagnosticError(jobResult.DiagnosticOutput);
					break;

				case CodeJobStatus.Completed:
					yield break;

				default:
					throw new InvalidOperationException("Unuspported code job status.");
			}
		}

		/// <summary>
		/// Return any compilation errors from the code job's class.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetClassCompilationErrors(
			CodeQuestionSubmission submission, 
			CodeJobResult jobResult)
		{
			if (jobResult.ClassCompilationResult == null
				|| (!jobResult.ClassCompilationResult.Success 
					&& jobResult.ClassCompilationResult.Errors.Count == 0))
			{
				return new List<CodeQuestionError>() { new NoClassError() };
			}

			return jobResult.ClassCompilationResult?.Errors?.Select
			(
				compilationError => GetFriendlyError
				(
					new ClassCompilationError
					(
						compilationError.LineNumber,
						compilationError.Message,
						compilationError.FullError
					)
				)
			) ?? Enumerable.Empty<CodeQuestionError>();
		}

		/// <summary>
		/// Returns any errors that prevented the job from executing.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetConstraintErrors(
			CodeQuestionSubmission submission)
		{
			if (Question.CodeConstraints == null)
				yield break;

			foreach (var constraint in Question.CodeConstraints)
			{
				int count = Regex.Matches(submission.Contents, constraint.Regex).Count;
				bool constraintMet;

				switch (constraint.Type)
				{
					case CodeConstraintType.AtLeast:
						constraintMet = count >= constraint.Frequency;
						break;

					case CodeConstraintType.Exactly:
						constraintMet = count == constraint.Frequency;
						break;

					case CodeConstraintType.AtMost:
						constraintMet = count <= constraint.Frequency;
						break;

					default:
						throw new InvalidOperationException("Invalid code constraint type.");
				}

				if (!constraintMet)
				{
					yield return new CodeConstraintError
					(
						constraint.Regex, 
						count, 
						constraint.ErrorMessage
					);
				}
			}
		}

		/// <summary>
		/// Return any compilation errors from the code job's tests.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetTestCompilationErrors(
			CodeQuestionSubmission submission, 
			CodeJobResult jobResult)
		{
			return jobResult.TestsCompilationResult?.Errors?.Select
			(
				compilationError => new TestCompilationError(compilationError.FullError)
			) ?? Enumerable.Empty<CodeQuestionError>();
		}

		/// <summary>
		/// Returns a list of test results for the job.
		/// </summary>
		private IEnumerable<CodeQuestionTestResult> GetTestResults(
			CodeJobResult jobResult, 
			CodeQuestionSubmission submission)
		{
			return jobResult.TestResults?.Select
			(
				testResult => new
				{
					Test = Question.GetTests().Single
					(
						test => GetCodeTestName(test) == testResult.Name
					),
					TestResult = testResult
				}
			)
			?.OrderBy
			(
				result => result.Test.Order
			)
			?.Select
			(
				result => new CodeQuestionTestResult
				(
					GetTestDescription(result.Test),
					result.Test.ExpectedOutput?.Replace("\r\n", "\n"),
					result.TestResult.Output?.Replace("\r\n", "\n"),
					result.Test.ExpectedReturnValue,
					result.TestResult.ReturnValue,
					result.TestResult.Exception,
					GetTestVisualizeUrl(result.Test, submission),
					GetTestSucceeded(result.Test, result.TestResult)
				)
			) ?? Enumerable.Empty<CodeQuestionTestResult>();
		}
		
		/// <summary>
		/// Returns the URL to visualize the execution of a test.
		/// </summary>
		private string GetTestVisualizeUrl(CodeQuestionTest test, CodeQuestionSubmission submission)
		{
			var urlEncodedCode = WebUtility.UrlEncode(GetVisualizerFileContents(test, submission));
			var urlEncodedArgs = GetVisualizerCommandLineArguments(test).Select(WebUtility.UrlEncode).ToList();
			var argsQueryStringVar = urlEncodedArgs.Any() 
				? $"&args={string.Join(",", urlEncodedArgs)}" 
				: string.Empty;

			return $"{c_visualizerBaseUrl}#code={urlEncodedCode}{argsQueryStringVar}";
		}

		/// <summary>
		/// Returns whether the test succeeded.
		/// </summary>
		private bool GetTestSucceeded(CodeQuestionTest test, CodeTestResult testResult)
		{
			var completedWithoutException = (testResult.Exception == null);
			var correctReturnValue = (testResult.ReturnValue == test.ExpectedReturnValue);
			var correctOutput = 
				   (testResult.Output?.TrimEveryLine() == test.ExpectedOutput?.TrimEveryLine()
				|| (string.IsNullOrEmpty(testResult.Output) && string.IsNullOrEmpty(test.ExpectedOutput)));

			return completedWithoutException && correctReturnValue && correctOutput;
		}

		/// <summary>
		/// Returns the contents of the java file to run with the java visualizer, for the given test.
		/// </summary>
		private string GetVisualizerFileContents(CodeQuestionTest test, CodeQuestionSubmission submission)
		{
			JavaFileBuilder builder = new JavaFileBuilder();

			if (Question.ImportedClasses != null)
				builder.AddLines(Question.ImportedClasses.Select(importedClass => $"import {importedClass.ClassName};"));

			builder.AddBlankLine();

			AddVisualizerClass(builder, test, submission);

			return builder.GetFileContents();
		}
	}
}
