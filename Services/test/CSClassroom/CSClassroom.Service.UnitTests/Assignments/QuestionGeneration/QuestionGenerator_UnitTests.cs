using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionGeneration
{
	/// <summary>
	/// Unit tests for the QuestionGenerator class.
	/// </summary>
	public class QuestionGenerator_UnitTests
	{
		/// <summary>
		/// Ensures that GenerateConstructorInvocation excludes the ID
		/// </summary>
		[Fact]
		public void GenerateConstructorInvocation_GeneratesCorrectInvocation()
		{
			var question = new MethodQuestion();
			var fileBuilder = new JavaFileBuilder();

			var questionGenerator = GetQuestionGenerator(question);
			questionGenerator.GenerateConstructorInvocation(question, fileBuilder);

			Assert.Equal("ConstructorInvocation\n", fileBuilder.GetFileContents());
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync calls the code runner service
		/// with a class job that has the correct class name.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_JobHasCorrectClassName()
		{
			var question = GetGeneratedQuestionTemplate();
			var codeRunnerService = GetMockCodeRunnerService();
			var questionGenerator = GetQuestionGenerator
			(
				question,
				codeRunnerService.Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question, 
				1 /*seed*/
			);

			codeRunnerService.Verify
			(
				crs => crs.ExecuteClassJobAsync
				(
					It.Is<ClassJob>
					(
						job => job.ClassName == "QuestionGenerator"
					)
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync calls the code runner service
		/// with a class job that has the correct import statements.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_JobHasCorrectImportStatements()
		{
			var question = GetGeneratedQuestionTemplate();
			var codeRunnerService = GetMockCodeRunnerService();
			var questionGenerator = GetQuestionGenerator
			(
				question,
				codeRunnerService.Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			codeRunnerService.Verify
			(
				crs => crs.ExecuteClassJobAsync
				(
					It.Is<ClassJob>
					(
						job => job.ClassesToImport.Contains("com.fasterxml.jackson.databind.ObjectMapper")
							&& job.ClassesToImport.Contains("com.fasterxml.jackson.databind.JsonMappingException")
							&& job.ClassesToImport.Contains("SpecifiedClassToImport")
					)
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync calls the code runner service
		/// with a class job that has the correct contents, when the
		/// contents were not cached previously on the question.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_NoCachedGeneratorFile_JobHasCorrectClassContents()
		{
			var question = GetGeneratedQuestionTemplate();
			var codeRunnerService = GetMockCodeRunnerService();
			var questionGenerator = GetQuestionGenerator
			(
				question,
				codeRunnerService.Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			codeRunnerService.Verify
			(
				crs => crs.ExecuteClassJobAsync
				(
					It.Is<ClassJob>
					(
						job => job.FileContents.Contains("GeneratorContents")
							&& job.FileContents.Contains("MethodQuestion")
							&& job.FileContents.Contains("ClassQuestion")
					)
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync calls the code runner service
		/// with a class job that has the correct contents, when the
		/// contents were previously cached on the question.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_CachedGeneratorFile_JobHasCorrectClassContents()
		{
			var question = GetGeneratedQuestionTemplate(cached: true);
			var codeRunnerService = GetMockCodeRunnerService();
			var questionGenerator = GetQuestionGenerator
			(
				question,
				codeRunnerService.Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			codeRunnerService.Verify
			(
				crs => crs.ExecuteClassJobAsync
				(
					It.Is<ClassJob>
					(
						job => job.FileContents == "CachedGeneratorFile"
					)
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync calls the code runner service
		/// with a class job that has the correct line offset, when the
		/// contents were not cached previously on the question.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_NoCachedGeneratorFile_JobHasCorrectLineOffset()
		{
			var question = GetGeneratedQuestionTemplate();
			var codeRunnerService = GetMockCodeRunnerService();
			var questionGenerator = GetQuestionGenerator
			(
				question,
				codeRunnerService.Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			codeRunnerService.Verify
			(
				crs => crs.ExecuteClassJobAsync
				(
					It.Is<ClassJob>
					(
						job => job.LineNumberOffset == -4
					)
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync calls the code runner service
		/// with a class job that has the correct line offset, when the
		/// contents were previously cached on the question.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_CachedGeneratorFile_JobHasCorrectLineOffset()
		{
			var question = GetGeneratedQuestionTemplate(cached: true);
			var codeRunnerService = GetMockCodeRunnerService();
			var questionGenerator = GetQuestionGenerator
			(
				question,
				codeRunnerService.Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			codeRunnerService.Verify
			(
				crs => crs.ExecuteClassJobAsync
				(
					It.Is<ClassJob>
					(
						job => job.LineNumberOffset == -100
					)
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync calls the code runner service
		/// with a class job that has the correct test to run.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_JobHasCorrectTest()
		{
			var question = GetGeneratedQuestionTemplate();
			var codeRunnerService = GetMockCodeRunnerService();
			var questionGenerator = GetQuestionGenerator
			(
				question,
				codeRunnerService.Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			codeRunnerService.Verify
			(
				crs => crs.ExecuteClassJobAsync
				(
					It.Is<ClassJob>
					(
						job => job.Tests.Count == 1
							&& job.Tests[0].TestName == "QuestionGenerator"
							&& job.Tests[0].ReturnType == "String"
					)
				),
				Times.Once
			);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator fails to compile.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_GeneratorCompilationError_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = new ClassJobResult()
			{
				ClassCompilationResult = new CompilationResult()
				{
					Success = false,
					Errors = Collections.CreateList
					(
						new CompileError() { FullError = "Error1" },
						new CompileError() { FullError = "Error2" }
					)
				}
			};

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Contains("Error1", result.Error);
			Assert.Contains("Error2", result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator does not have a method with the required name.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_WrongMethodName_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();
			classJobResult.ClassDefinition.Methods[0].Name = "WrongMethodName";

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator does not have a public generateQuestion method.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_NonPublicMethod_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();
			classJobResult.ClassDefinition.Methods[0].IsPublic = false;

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator does not have a static generateQuestion method.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_NonStaticMethod_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();
			classJobResult.ClassDefinition.Methods[0].IsStatic = false;

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator does not have a generateQuestion method taking a single
		/// integer parameter for the seed.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_WrongParamTypes_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();
			classJobResult.ClassDefinition.Methods[0].ParameterTypes 
				= Collections.CreateList("int", "String");

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator does not have a generateQuestion method returning a type
		/// of question.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_WrongReturnType_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();
			classJobResult.ClassDefinition.Methods[0].ReturnType = "String";

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator's test does not compile.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_TestCompilationFailure_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();
			classJobResult.TestsCompilationResult = new CompilationResult
			{
				Success = false,
				Errors = Collections.CreateList
				(
					new CompileError() { FullError = "Error1" },
					new CompileError() { FullError = "Error2" }
				)
			};

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Contains("Error1", result.Error);
			Assert.Contains("Error2", result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns an error when the submitted 
		/// generator's test throws an exception.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_TestException_ReturnsError()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();
			classJobResult.TestResults = Collections.CreateList
			(
				new CodeTestResult()
				{
					Name = "QuestionGenerator",
					Completed = false,
					Exception = "TestException"
				}
			);

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.NotNull(result.Error);
			Assert.Contains("TestException", result.Error);
			Assert.Null(result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns the serialized question without error
		/// when the submitted generator is valid.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_ValidGenerator_ReturnsSerializedQuestion()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.Null(result.Error);
			Assert.Equal("SerializedQuestion", result.SerializedQuestion);
		}

		/// <summary>
		/// Ensures GenerateQuestionAsync returns the full generator file contents
		/// so that it can be cached.
		/// </summary>
		[Fact]
		public async Task GenerateQuestionAsync_ValidGenerator_ReturnsFullGeneratorFileContents()
		{
			var question = GetGeneratedQuestionTemplate();
			var classJobResult = GetSuccessfulClassJobResult();

			var questionGenerator = GetQuestionGenerator
			(
				question,
				GetMockCodeRunnerService(classJobResult).Object
			);

			var result = await questionGenerator.GenerateQuestionAsync
			(
				question,
				1 /*seed*/
			);

			Assert.Null(result.Error);
			Assert.Contains("GeneratorContents", result.FullGeneratorFileContents);
			Assert.Contains("MethodQuestion", result.FullGeneratorFileContents);
			Assert.Contains("ClassQuestion", result.FullGeneratorFileContents);
			Assert.Equal(-4, result.FullGeneratorFileLineOffset);
		}

		/// <summary>
		/// Returns a new generated question template.
		/// </summary>
		private GeneratedQuestionTemplate GetGeneratedQuestionTemplate(
			bool cached = false)
		{
			return new GeneratedQuestionTemplate()
			{
				ImportedClasses = Collections.CreateList
				(
					new ImportedClass()
					{
						ClassName = "SpecifiedClassToImport"
					}
				),

				GeneratorContents = "GeneratorContents",

				FullGeneratorFileContents = cached
					? "CachedGeneratorFile"
					: null,

				FullGeneratorFileLineOffset = -100
			};
		}

		/// <summary>
		/// Returns a mock java model.
		/// </summary>
		private IList<JavaClass> GetJavaModel()
		{
			return Collections.CreateList
			(
				new JavaClass(typeof(MethodQuestion), null, null),
				new JavaClass(typeof(ClassQuestion), null, null)
			);
		}

		/// <summary>
		/// Returns a new question generator.
		/// </summary>
		private IQuestionGenerator GetQuestionGenerator(
			Question question,
			ICodeRunnerService codeRunnerService = null)
		{
			var javaModel = GetJavaModel();

			var codeGenFactory = GetMockCodeGenerationFactory
			(
				question,
				javaModel
			);

			var questionModelFactory = GetMockQuestionModelFactory
			(
				javaModel
			);

			var questionGenerator = new QuestionGenerator
			(
				codeGenFactory.Object,
				questionModelFactory.Object,
				codeRunnerService
			);

			return questionGenerator;
		}

		/// <summary>
		/// Returns a mock code generation factory.
		/// </summary>
		private Mock<IJavaCodeGenerationFactory> GetMockCodeGenerationFactory(
			Question question,
			IList<JavaClass> javaModel)
		{
			var codeGenerationFactory = new Mock<IJavaCodeGenerationFactory>();

			codeGenerationFactory
				.Setup
				(
					factory => factory.CreateConstructorInvocationGenerator
					(
						It.IsAny<JavaFileBuilder>(),
						javaModel
					)
				)
				.Returns<JavaFileBuilder, IList<JavaClass>>
				(
					(builder, model) => 
						GetMockJavaConstructorInvocationGenerator(builder, question)
				);

			codeGenerationFactory
				.Setup
				(
					factory => factory.CreateJavaClassDefinitionGenerator
					(
						It.IsAny<JavaFileBuilder>(),
						It.Is<JavaClass>
						(
							javaClass => javaModel.Contains(javaClass)
						)
					)
				)
				.Returns<JavaFileBuilder, JavaClass>(GetMockJavaClassDefinitionGenerator);

			return codeGenerationFactory;
		}

		/// <summary>
		/// Returns a mock java constructor invocation generator.
		/// </summary>
		private static IJavaConstructorInvocationGenerator GetMockJavaConstructorInvocationGenerator(
			JavaFileBuilder builder,
			Question question)
		{
			var generator = new Mock<IJavaConstructorInvocationGenerator>();

			generator
				.Setup
				(
					g => g.GenerateConstructorInvocation
					(
						question,
						"return ",
						";"
					)
				)
				.Callback<object, string, string>
				(
					(obj, prefix, suffix) => builder.AddLine("ConstructorInvocation")
				);

			return generator.Object;
		}

		/// <summary>
		/// Returns a mock java class definition generator.
		/// </summary>
		private static IJavaClassDefinitionGenerator GetMockJavaClassDefinitionGenerator(
			JavaFileBuilder builder,
			JavaClass javaClass)
		{
			var generator = new Mock<IJavaClassDefinitionGenerator>();

			generator
				.Setup
				(
					g => g.GenerateClassDefinition()
				)
				.Callback
				(
					() => builder.AddLine(javaClass.ClassName)
				);

			return generator.Object;
		}

		/// <summary>
		/// Returns the expression for generating a constructor invocation.
		/// </summary>
		private static Expression<Action<IJavaCodeGenerationFactory>> GetGenerateConstructorExpression(
			Question question, 
			IList<JavaClass> javaModel)
		{
			return factory => factory.CreateConstructorInvocationGenerator
			(
				It.IsAny<JavaFileBuilder>(),
				javaModel
			)
			.GenerateConstructorInvocation
			(
				question,
				"return ",
				";"
			);
		}

		/// <summary>
		/// Returns a mock question model factory.
		/// </summary>
		private Mock<IQuestionModelFactory> GetMockQuestionModelFactory(
			IList<JavaClass> javaModel)
		{
			var questionModelFactory = new Mock<IQuestionModelFactory>();

			questionModelFactory
				.Setup(factory => factory.GetQuestionModel())
				.Returns(javaModel);

			return questionModelFactory;
		}

		/// <summary>
		/// Returns a mock code runner service.
		/// </summary>
		private Mock<ICodeRunnerService> GetMockCodeRunnerService(
			ClassJobResult jobResult = null)
		{
			jobResult = jobResult ?? GetSuccessfulClassJobResult();

			var codeRunnerService = new Mock<ICodeRunnerService>();

			codeRunnerService
				.Setup
				(
					crs => crs.ExecuteClassJobAsync
					(
						It.IsAny<ClassJob>()
					)
				).ReturnsAsync(jobResult);

			return codeRunnerService;
		}

		/// <summary>
		/// Returns a successful class job result.
		/// </summary>
		private ClassJobResult GetSuccessfulClassJobResult()
		{
			return new ClassJobResult()
			{
				ClassCompilationResult = new CompilationResult()
				{
					Success = true
				},
				ClassDefinition = new ClassDefinition()
				{
					Name = "QuestionGenerator",
					Methods = Collections.CreateList
					(
						new MethodDefinition()
						{
							Name = "generateQuestion",
							IsPublic = true,
							IsStatic = true,
							ParameterTypes = Collections.CreateList("int"),
							ReturnType = "Question"
						}
					)
				},
				TestsCompilationResult = new CompilationResult()
				{
					Success = true
				},
				TestResults = Collections.CreateList
				(
					new CodeTestResult()
					{
						Name = "QuestionGenerator",
						Completed = true,
						ReturnValue = "SerializedQuestion"
					}
				)
			};
		}

		/// <summary>
		/// Returns a class job result with an incorrect method definition.
		/// </summary>
		private ClassJobResult GetClassJobResultWrongDefinition(MethodDefinition def)
		{
			return new ClassJobResult()
			{
				ClassCompilationResult = new CompilationResult()
				{
					Success = true,
				},
				ClassDefinition = new ClassDefinition
				{
					Name = "QuestionGenerator",
					Methods = Collections.CreateList(def)
				}
			};
		}
	}
}
