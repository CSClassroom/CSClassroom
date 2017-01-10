using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Questions.ServiceResults.Errors;
using CSC.CSClassroom.Service.Questions.QuestionGraders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionGraders
{
	/// <summary>
	/// Unit tests for the ProgramQuestionGrader class.
	/// </summary>
	public class ProgramQuestionGrader_UnitTests
	{
		/// <summary>
		/// Verifies that the created class job has imported classes.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasImportedClasses()
		{
			var question = GetProgramQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.ClassesToImport.Count == 1
					&& job.ClassesToImport[0] == "package.classToImport"
			);

			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the correct class name.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasCorrectClassName()
		{
			var question = GetProgramQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.ClassName == "ExpectedProgram"
			);

			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the correct file contents.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasCorrectFileContents()
		{
			var question = GetProgramQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission %" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.FileContents == "Submission %%"
			);

			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the correct line offset.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasCorrectLineOffset()
		{
			var question = GetProgramQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.LineNumberOffset == 0
			);

			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the desired tests.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasTests()
		{
			var question = GetProgramQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission %" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.Tests.Count == 1
					&& job.Tests[0].TestName == "test1"
					&& job.Tests[0].MethodBody == "ExpectedProgram.main(new String[] {\"One\",\"Two\"});"
					&& job.Tests[0].ReturnType == "void"
			);

			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission does not
		/// contain any classes.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MissingExpectedClass_Error()
		{
			var question = GetProgramQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.TestsCompilationResult = null;
			classJobResult.ClassDefinition = null;
			classJobResult.ClassCompilationResult = new CompilationResult()
			{
				Success = false,
				Errors = Collections.CreateList
				(
					new CompileError()
					{
						Message = "class ExpectedProgram should be declared in ExpectedProgram.java"
					}
				)
			};

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var missingRequiredClassError = codeQuestionResult.Errors
				.Cast<MissingRequiredClassError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedProgram", missingRequiredClassError.RequiredClassName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission does not
		/// contain a main method.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MainMethodMissing_Error()
		{
			var question = GetProgramQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods.Clear();

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var mainMethodMissingError = codeQuestionResult.Errors
				.Cast<MainMethodMissingError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedProgram", mainMethodMissingError.ClassName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a non-public main method.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NonPublicMainMethod_Error()
		{
			var question = GetProgramQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].IsPublic = false;

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var mainMethodMissingError = codeQuestionResult.Errors
				 .Cast<MainMethodMissingError>()
				 .Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedProgram", mainMethodMissingError.ClassName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a non-static main method.  
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_UnexpectedStaticMethod_Error()
		{
			var question = GetProgramQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].IsStatic = false;

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var mainMethodMissingError = codeQuestionResult.Errors
				 .Cast<MainMethodMissingError>()
				 .Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedProgram", mainMethodMissingError.ClassName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a main method with a non-void return type.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NonVoidMainMethod_Error()
		{
			var question = GetProgramQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].ReturnType = "boolean";

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var mainMethodMissingError = codeQuestionResult.Errors
				 .Cast<MainMethodMissingError>()
				 .Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedProgram", mainMethodMissingError.ClassName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a main method with invalid parameter types.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongMainMethodParameterTypes_Error()
		{
			var question = GetProgramQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].ParameterTypes[0] = "double";

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var mainMethodMissingError = codeQuestionResult.Errors
				 .Cast<MainMethodMissingError>()
				 .Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedProgram", mainMethodMissingError.ClassName);
		}

		/// <summary>
		/// Verifies that a valid test description is returned when there
		/// are no definition errors.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_CorrectSubmission_ValidTestDescription()
		{
			var question = GetProgramQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService(classJobResult);

			var grader = new ProgramQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testResult = codeQuestionResult.TestResults.Single();

			Assert.Equal(1.0, result.Score);
			Assert.Equal(0, codeQuestionResult.Errors.Count);
			Assert.Equal("Description", testResult.Description);
		}

		/// <summary>
		/// Returns a new program question.
		/// </summary>
		public ProgramQuestion GetProgramQuestion()
		{
			return new ProgramQuestion()
			{
				ProgramClassName = "ExpectedProgram",

				ImportedClasses = Collections.CreateList
				(
					new ImportedClass() { ClassName = "package.classToImport" }
				),

				Tests = Collections.CreateList
				(
					new ProgramQuestionTest()
					{
						Name = $"test1",
						TestDescription = "Description",
						Order = 1,
						CommandLineArguments = "One Two",
						ExpectedOutput = "expectedOutput"
					}
				)
			};
		}

		/// <summary>
		/// Returns a successful class job result.
		/// </summary>
		public ClassJobResult GetClassJobResult(bool success)
		{
			return new ClassJobResult()
			{
				Status = CodeJobStatus.Completed,

				ClassCompilationResult = new CompilationResult() { Success = true },

				ClassDefinition = new ClassDefinition()
				{
					Name = "ExpectedProgram",
					Fields = new List<FieldDefinition>(),
					Methods = Collections.CreateList
					(
						new MethodDefinition()
						{
							Name = "main",
							IsPublic = true,
							IsStatic = true,
							ParameterTypes = Collections.CreateList("String[]"),
							ReturnType = "void"
						}
					)
				},

				TestsCompilationResult = success
					? new CompilationResult() { Success = true }
					: new CompilationResult()
						{
							Success = false,
							Errors = Collections.CreateList
							(
								new CompileError()
								{
									FullError = "Test compilation failure"
								}
							)
						},

				TestResults = success
					? Collections.CreateList
						(
							new CodeTestResult()
							{
								Name = "test1",
								Completed = true,
								Output = "expectedOutput"
							}
						)
					: null
			};
		}

		/// <summary>
		/// Returns a code runner service that responds with the 
		/// given result, when called with the given job.
		/// </summary>
		public ICodeRunnerService GetCodeRunnerService(
			ClassJobResult expectedClassJobResult,
			Expression<Func<ClassJob, bool>> expectedClassJob = null)
		{
			if (expectedClassJob == null)
			{
				expectedClassJob = job => true;
			}

			var codeRunnerService = new Mock<ICodeRunnerService>();

			codeRunnerService
				.Setup(crs => crs.ExecuteClassJobAsync(It.Is(expectedClassJob)))
				.ReturnsAsync(expectedClassJobResult);

			return codeRunnerService.Object;
		}
	}
}

