using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Assignments.ServiceResults.Errors;
using CSC.CSClassroom.Service.Assignments.QuestionGraders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionGraders
{
	/// <summary>
	/// Unit tests for the MethodQuestionGrader class.
	/// </summary>
	public class MethodQuestionGrader_UnitTests
	{
		/// <summary>
		/// Verifies that the created method job has imported classes.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MethodJobHasImportedClasses()
		{
			var question = GetMethodQuestion();
			var methodJobResult = GetSuccessfulMethodJobResult();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService
			(
				methodJobResult,
				job => job.ClassesToImport.Count == 1
					&& job.ClassesToImport[0] == "package.classToImport"
			);

			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created method job has the submitted method code.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MethodJobHasCorrectCode()
		{
			var question = GetMethodQuestion();
			var methodJobResult = GetSuccessfulMethodJobResult();
			var submission = new CodeQuestionSubmission() { Contents = "Submission %" };
			var codeRunnerService = GetCodeRunnerService
			(
				methodJobResult,
				job => job.MethodCode == "Submission %%"
			);

			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created method job has the desired tests.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MethodJobHasTests()
		{
			var question = GetMethodQuestion();
			var methodJobResult = GetSuccessfulMethodJobResult();
			var submission = new CodeQuestionSubmission() { Contents = "Submission %" };
			var codeRunnerService = GetCodeRunnerService
			(
				methodJobResult,
				job => job.Tests.Count == 1
					&& job.Tests[0].TestName == "test1"
					&& job.Tests[0].ParamValues == "1, 2"
			);

			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission does not
		/// contain any method.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MissingMethod_Error()
		{
			var question = GetMethodQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var methodJobResult = GetFailedMethodJobResult(definition: null);
			var codeRunnerService = GetCodeRunnerService(methodJobResult);

			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodMissingError = codeQuestionResult.Errors
				.Cast<MethodMissingError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("expectedMethod", methodMissingError.ExpectedMethodName);
			Assert.True(methodMissingError.ExpectedStatic);
		}
		
		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a method with the wrong name.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongMethodName_Error()
		{
			var question = GetMethodQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var methodJobResult = GetFailedMethodJobResult
			(
				new MethodDefinition()
				{
					IsPublic = true,
					IsStatic = true,
					Name = "wrongMethod",
					ParameterTypes = Collections.CreateList("int", "int"),
					ReturnType = "int"
				}
			);

			var codeRunnerService = GetCodeRunnerService(methodJobResult);
			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodNameError = codeQuestionResult.Errors
				.Cast<MethodNameError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("expectedMethod", methodNameError.ExpectedMethodName);
			Assert.Equal("wrongMethod", methodNameError.ActualMethodName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a method that is not public.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NonPublicMethod_Error()
		{
			var question = GetMethodQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var methodJobResult = GetFailedMethodJobResult
			(
				new MethodDefinition()
				{
					IsPublic = false,
					IsStatic = true,
					Name = "expectedMethod",
					ParameterTypes = Collections.CreateList("int", "int"),
					ReturnType = "int"
				}
			);

			var codeRunnerService = GetCodeRunnerService(methodJobResult);
			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodVisibilityError = codeQuestionResult.Errors
				.Cast<MethodVisibilityError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("expectedMethod", methodVisibilityError.MethodName);
			Assert.True(methodVisibilityError.ExpectedPublic);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a method that is not static.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NonStaticMethod_Error()
		{
			var question = GetMethodQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var methodJobResult = GetFailedMethodJobResult
			(
				new MethodDefinition()
				{
					IsPublic = true,
					IsStatic = false,
					Name = "expectedMethod",
					ParameterTypes = Collections.CreateList("int", "int"),
					ReturnType = "int"
				}
			);

			var codeRunnerService = GetCodeRunnerService(methodJobResult);
			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodStaticError = codeQuestionResult.Errors
				.Cast<MethodStaticError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("expectedMethod", methodStaticError.MethodName);
			Assert.True(methodStaticError.ExpectedStatic);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a method with an incorrect return type.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongReturnType_Error()
		{
			var question = GetMethodQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var methodJobResult = GetFailedMethodJobResult
			(
				new MethodDefinition()
				{
					IsPublic = true,
					IsStatic = true,
					Name = "expectedMethod",
					ParameterTypes = Collections.CreateList("int", "int"),
					ReturnType = "boolean"
				}
			);

			var codeRunnerService = GetCodeRunnerService(methodJobResult);
			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodReturnTypeError = codeQuestionResult.Errors
				.Cast<MethodReturnTypeError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("expectedMethod", methodReturnTypeError.MethodName);
			Assert.Equal("int", methodReturnTypeError.ExpectedReturnType);
			Assert.Equal("boolean", methodReturnTypeError.ActualReturnType);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a method with incorrect parameter types.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongParameterTypes_Error()
		{
			var question = GetMethodQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var methodJobResult = GetFailedMethodJobResult
			(
				new MethodDefinition()
				{
					IsPublic = true,
					IsStatic = true,
					Name = "expectedMethod",
					ParameterTypes = Collections.CreateList("int", "double"),
					ReturnType = "int"
				}
			);

			var codeRunnerService = GetCodeRunnerService(methodJobResult);
			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodParameterTypesError = codeQuestionResult.Errors
				.Cast<MethodParameterTypesError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("expectedMethod", methodParameterTypesError.MethodName);
			Assert.Equal(2, methodParameterTypesError.ExpectedParamTypes.Count);
			Assert.Equal("int", methodParameterTypesError.ExpectedParamTypes[0]);
			Assert.Equal("int", methodParameterTypesError.ExpectedParamTypes[1]);
			Assert.Equal("int", methodParameterTypesError.ActualParamTypes[0]);
			Assert.Equal("double", methodParameterTypesError.ActualParamTypes[1]);
		}

		/// <summary>
		/// Verifies that a valid test description is returned when there
		/// are no definition errors.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_CorrectSubmission_ValidTestDescription()
		{
			var question = GetMethodQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var methodJobResult = GetSuccessfulMethodJobResult();
			var codeRunnerService = GetCodeRunnerService(methodJobResult);

			var grader = new MethodQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testResult = codeQuestionResult.TestResults.Single();

			Assert.Equal(1.0, result.Score);
			Assert.Equal(0, codeQuestionResult.Errors.Count);
			Assert.Equal("expectedMethod(1, 2)", testResult.Description);
		}

		/// <summary>
		/// Returns a new method question.
		/// </summary>
		public MethodQuestion GetMethodQuestion()
		{
			return new MethodQuestion()
			{
				MethodName = "expectedMethod",
				ImportedClasses = Collections.CreateList
				(
					new ImportedClass() { ClassName = "package.classToImport" }
				),
				ParameterTypes = "int, int",
				ReturnType = "int",
				Tests = Collections.CreateList
				(
					new MethodQuestionTest()
					{
						Name = $"test1",
						Order = 1,
						ParameterValues = "1, 2",
						ExpectedReturnValue = "expectedReturnValue",
						ExpectedOutput = "expectedOutput"
					}
				)
			};
		}

		/// <summary>
		/// Returns a successful method job result.
		/// </summary>
		public MethodJobResult GetSuccessfulMethodJobResult()
		{
			return new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				MethodDefinition = new MethodDefinition()
				{
					Name = "expectedMethod",
					IsPublic = true,
					IsStatic = true,
					ParameterTypes = Collections.CreateList("int", "int"),
					ReturnType = "int"
				},
				TestsCompilationResult = new CompilationResult() { Success = true },
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "test1",
						Completed = true,
						Output = "expectedOutput",
						ReturnValue = "expectedReturnValue"
					}
				)
			};
		}

		/// <summary>
		/// Returns a failed method job result.
		/// </summary>
		public MethodJobResult GetFailedMethodJobResult(
			MethodDefinition definition)
		{
			return new MethodJobResult()
			{
				Status = CodeJobStatus.Completed,
				ClassCompilationResult = new CompilationResult() { Success = true },
				MethodDefinition = definition,
				TestsCompilationResult = new CompilationResult()
				{
					Success = false,
					Errors = Collections.CreateList
					(
						new CompileError()
						{
							FullError = "Test compilation failure"
						}
					)
				}
			};
		}

		/// <summary>
		/// Returns a code runner service that responds with the 
		/// given result, when called with the given job.
		/// </summary>
		public ICodeRunnerService GetCodeRunnerService(
			MethodJobResult expectedMethodJobResult,
			Expression<Func<MethodJob, bool>> expectedMethodJob = null)
		{
			if (expectedMethodJob == null)
			{
				expectedMethodJob = job => true;
			}

			var codeRunnerService = new Mock<ICodeRunnerService>();

			codeRunnerService
				.Setup(crs => crs.ExecuteMethodJobAsync(It.Is(expectedMethodJob)))
				.ReturnsAsync(expectedMethodJobResult);

			return codeRunnerService.Object;
		}
	}
}

