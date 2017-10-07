using System;
using System.Collections.Generic;
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
	/// Unit tests for the ClassQuestionGrader class.
	/// </summary>
	public class ClassQuestionGrader_UnitTests
	{
		/// <summary>
		/// Verifies that the created class job has imported classes.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasImportedClasses()
		{
			var question = GetClassQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.ClassesToImport.Count == 1
					&& job.ClassesToImport[0] == "package.classToImport"
			);

			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the correct class name.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasCorrectClassName()
		{
			var question = GetClassQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.ClassName == "ExpectedClass"
			);

			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the correct file contents.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasCorrectFileContents()
		{
			var question = GetClassQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission %" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.FileContents ==
					"class InternalClass\n" +
					"{\n" +
					"}\n" +
					"\n"
					+ "Submission %%"
			);

			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the correct line offset.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasCorrectLineOffset()
		{
			var question = GetClassQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.LineNumberOffset == -4
			);

			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Verifies that the created class job has the desired tests.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ClassJobHasTests()
		{
			var question = GetClassQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission %" };
			var codeRunnerService = GetCodeRunnerService
			(
				classJobResult,
				job => job.Tests.Count == 1
					&& job.Tests[0].TestName == "test1"
					&& job.Tests[0].MethodBody == "Method Body"
					&& job.Tests[0].ReturnType == "String"
			);

			var grader = new ClassQuestionGrader(question, codeRunnerService);
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
			var question = GetClassQuestion();
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
						Message = "class ExpectedClass should be declared in ExpectedClass.java"
					}
				)
			};

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var missingRequiredClassError = codeQuestionResult.Errors
				.Cast<MissingRequiredClassError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedClass", missingRequiredClassError.RequiredClassName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission does not
		/// contain any classes.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_ForbiddenPublicFields_Error()
		{
			var question = GetClassQuestion(allowPublicFields: false);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Fields = new List<FieldDefinition>()
			{
				new FieldDefinition()
				{
					Name = "field1",
					IsPublic = true,
					Type = "String"
				},
				new FieldDefinition()
				{
					Name = "field2",
					IsPublic = false,
					Type = "String"
				}
			};

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var fieldVisibilityError = codeQuestionResult.Errors
				.Cast<FieldVisibilityError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedClass", fieldVisibilityError.ClassName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission does not
		/// contain any classes.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_PermittedPublicFields_Success()
		{
			var question = GetClassQuestion(allowPublicFields: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: true);
			classJobResult.ClassDefinition.Fields = new List<FieldDefinition>()
			{
				new FieldDefinition()
				{
					Name = "field1",
					IsPublic = true,
					Type = "String"
				},
				new FieldDefinition()
				{
					Name = "field2",
					IsPublic = false,
					Type = "String"
				}
			};

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;

			Assert.Equal(1.0, result.Score);
			Assert.Empty(codeQuestionResult.Errors);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission does not
		/// contain a required method.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MissingRequiredMethod_Error()
		{
			var question = GetClassQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods.Clear();

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodMissingError = codeQuestionResult.Errors
				.Cast<MethodMissingError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedClass", methodMissingError.ClassName);
			Assert.Equal("requiredMethod", methodMissingError.ExpectedMethodName);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a required method with incorrect visibility.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongMethodVisibility_Error()
		{
			var question = GetClassQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].IsPublic = false;

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodVisibilityError = codeQuestionResult.Errors
				.Cast<MethodVisibilityError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("requiredMethod", methodVisibilityError.MethodName);
			Assert.True(methodVisibilityError.ExpectedPublic);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a required method that is unexpectedly static.  
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_UnexpectedStaticMethod_Error()
		{
			var question = GetClassQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].IsStatic = true;

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodStaticError = codeQuestionResult.Errors
				.Cast<MethodStaticError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("requiredMethod", methodStaticError.MethodName);
			Assert.False(methodStaticError.ExpectedStatic);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a required method with an incorrect return type.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongReturnType_Error()
		{
			var question = GetClassQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].ReturnType = "boolean";

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodReturnTypeError = codeQuestionResult.Errors
				.Cast<MethodReturnTypeError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("requiredMethod", methodReturnTypeError.MethodName);
			Assert.Equal("String", methodReturnTypeError.ExpectedReturnType);
			Assert.Equal("boolean", methodReturnTypeError.ActualReturnType);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// a required method with incorrect parameter types.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongParameterTypes_Error()
		{
			var question = GetClassQuestion();
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false);
			classJobResult.ClassDefinition.Methods[0].ParameterTypes[1] = "double";

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodParameterTypesError = codeQuestionResult.Errors
				.Cast<MethodParameterTypesError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("requiredMethod", methodParameterTypesError.MethodName);
			Assert.Equal(2, methodParameterTypesError.ExpectedParamTypes.Count);
			Assert.Equal("int", methodParameterTypesError.ExpectedParamTypes[0]);
			Assert.Equal("int", methodParameterTypesError.ExpectedParamTypes[1]);
			Assert.Equal("int", methodParameterTypesError.ActualParamTypes[0]);
			Assert.Equal("double", methodParameterTypesError.ActualParamTypes[1]);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// an incorrect number of overloads for a required overloaded method.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongOverloadCount_Error()
		{
			var question = GetClassQuestion(overloadedMethods: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false, overloadedMethods: true);
			classJobResult.ClassDefinition.Methods.RemoveAt(0);

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodOverloadCountError = codeQuestionResult.Errors
				.Cast<MethodOverloadCountError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedClass", methodOverloadCountError.ClassName);
			Assert.Equal("requiredOverloadedMethod", methodOverloadCountError.ExpectedMethodName);
			Assert.Equal(2, methodOverloadCountError.ExpectedOverloadCount);
			Assert.False(methodOverloadCountError.ExpectedStatic);
		}

		/// <summary>
		/// Verifies that an error is returned if the submission contains
		/// an overloaded method with an unexpected signature.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_WrongOverloadSignature_Error()
		{
			var question = GetClassQuestion(overloadedMethods: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };

			var classJobResult = GetClassJobResult(success: false, overloadedMethods: true);
			classJobResult.ClassDefinition.Methods[0].ParameterTypes[0] = "String";

			var codeRunnerService = GetCodeRunnerService(classJobResult);
			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var methodOverloadDefinitionError = codeQuestionResult.Errors
				.Cast<MethodOverloadDefinitionError>()
				.Single();

			Assert.Equal(0.0, result.Score);
			Assert.Equal("ExpectedClass", methodOverloadDefinitionError.ClassName);
			Assert.Equal("requiredOverloadedMethod", methodOverloadDefinitionError.ExpectedMethodName);
			Assert.True(methodOverloadDefinitionError.ExpectedPublic);
			Assert.False(methodOverloadDefinitionError.ExpectedStatic);
			Assert.Equal("int", methodOverloadDefinitionError.ExpectedParamTypes);
			Assert.Equal("boolean", methodOverloadDefinitionError.ExpectedReturnType);
		}

		/// <summary>
		/// Verifies that a valid test description is returned when there
		/// are no definition errors.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_CorrectSubmission_ValidTestDescription()
		{
			var question = GetClassQuestion();
			var classJobResult = GetClassJobResult(success: true);
			var submission = new CodeQuestionSubmission() { Contents = "Submission" };
			var codeRunnerService = GetCodeRunnerService(classJobResult);

			var grader = new ClassQuestionGrader(question, codeRunnerService);
			var result = await grader.GradeSubmissionAsync(submission);
			var codeQuestionResult = (CodeQuestionResult)result.Result;
			var testResult = codeQuestionResult.TestResults.Single();

			Assert.Equal(1.0, result.Score);
			Assert.Empty(codeQuestionResult.Errors);
			Assert.Equal("Description", testResult.Description);
		}

		/// <summary>
		/// Returns a new class question.
		/// </summary>
		public ClassQuestion GetClassQuestion(
			bool allowPublicFields = false, 
			bool overloadedMethods = false)
		{
			return new ClassQuestion()
			{
				ClassName = "ExpectedClass",

				FileTemplate = 
					"class InternalClass\n" +
					"{\n" +
					"}\n" +
					"\n"
					+ "%SUBMISSION%",

				AllowPublicFields = allowPublicFields,

				ImportedClasses = Collections.CreateList
				(
					new ImportedClass() { ClassName = "package.classToImport" }
				),

				RequiredMethods = overloadedMethods
					? Collections.CreateList
						(
							new RequiredMethod()
							{
								Name = "requiredOverloadedMethod",
								IsPublic = true,
								IsStatic = false,
								ParamTypes = "int",
								ReturnType = "boolean"
							},
							new RequiredMethod()
							{
								Name = "requiredOverloadedMethod",
								IsPublic = true,
								IsStatic = false,
								ParamTypes = "double",
								ReturnType = "String"
							}
						)
					: Collections.CreateList
						(
							new RequiredMethod()
							{
								Name = "requiredMethod",
								IsPublic = true,
								IsStatic = false,
								ParamTypes = "int, int",
								ReturnType = "String"
							}
						),

				Tests = Collections.CreateList
				(
					new ClassQuestionTest()
					{
						Name = $"test1",
						Order = 1,
						Description = "Description",
						MethodBody = "Method Body",
						ReturnType = "String",
						ExpectedReturnValue = "expectedReturnValue",
						ExpectedOutput = "expectedOutput"
					}
				)
			};
		}

		/// <summary>
		/// Returns a successful class job result.
		/// </summary>
		public ClassJobResult GetClassJobResult(bool success, bool overloadedMethods = false)
		{
			return new ClassJobResult()
			{
				Status = CodeJobStatus.Completed,

				ClassCompilationResult = new CompilationResult() { Success = true },

				ClassDefinition = new ClassDefinition()
				{
					Name = "ExpectedClass",
					Fields = new List<FieldDefinition>(),
					Methods = overloadedMethods
						? Collections.CreateList
							(
								new MethodDefinition()
								{
									Name = "requiredOverloadedMethod",
									IsPublic = true,
									IsStatic = false,
									ParameterTypes = Collections.CreateList("int"),
									ReturnType = "boolean"
								},
								new MethodDefinition()
								{
									Name = "requiredOverloadedMethod",
									IsPublic = true,
									IsStatic = false,
									ParameterTypes = Collections.CreateList("double"),
									ReturnType = "String"
								}
							)
						: Collections.CreateList
							(
								new MethodDefinition()
								{
									Name = "requiredMethod",
									IsPublic = true,
									IsStatic = false,
									ParameterTypes = Collections.CreateList("int", "int"),
									ReturnType = "String"
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
								Output = "expectedOutput",
								ReturnValue = "expectedReturnValue"
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

