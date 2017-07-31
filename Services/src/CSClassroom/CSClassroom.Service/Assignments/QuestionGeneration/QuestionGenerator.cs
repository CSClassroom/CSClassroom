using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// Generates a specific instance of a question, from a generated question.
	/// </summary>
	public class QuestionGenerator : IQuestionGenerator
	{
		/// <summary>
		/// The java code generation factory.
		/// </summary>
		private readonly IJavaCodeGenerationFactory _javaCodeGenerationFactory;

		/// <summary>
		/// The question model factory.
		/// </summary>
		private readonly IQuestionModelFactory _questionModelFactory;

		/// <summary>
		/// The code runner service.
		/// </summary>
		private readonly ICodeRunnerService _codeRunnerService;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionGenerator(
			IJavaCodeGenerationFactory javaCodeGenerationFactory,
			IQuestionModelFactory questionModelFactory,
			ICodeRunnerService codeRunnerService)
		{
			_javaCodeGenerationFactory = javaCodeGenerationFactory;
			_questionModelFactory = questionModelFactory;
			_codeRunnerService = codeRunnerService;
		}

		/// <summary>
		/// Generates an invocation of the question constructor that
		/// populates the resulting question with all of the existing 
		/// question's data.
		/// </summary>
		public void GenerateConstructorInvocation(
			Question existingQuestion,
			JavaFileBuilder fileBuilder)
		{
			var javaModel = _questionModelFactory.GetQuestionModel();

			var constructorInvocationGenerator = _javaCodeGenerationFactory
				.CreateConstructorInvocationGenerator(fileBuilder, javaModel);

			constructorInvocationGenerator.GenerateConstructorInvocation
			(
				existingQuestion,
				prefix: "return ",
				suffix: ";"
			);
		}

		/// <summary>
		/// Generates a specific question given a generated question and a seed.
		/// </summary>
		public async Task<QuestionGenerationResult> GenerateQuestionAsync(
			GeneratedQuestionTemplate question, 
			int seed)
		{
			var classJob = CreateQuestionGeneratorClassJob(question, seed);

			var classJobResult = await _codeRunnerService.ExecuteClassJobAsync(classJob);

			if (!classJobResult.ClassCompilationResult.Success)
			{
				return new QuestionGenerationResult
				(
					error: "Failed to compile question generator. Compilation errors: \n"
						+ string.Join(
							"\n",
							classJobResult.ClassCompilationResult.Errors.Select(error => error.FullError))
				);
			}

			if (!classJobResult.ClassDefinition.Methods.Any(
					m =>   m.IsStatic
						&& m.IsPublic
						&& m.ReturnType.EndsWith("Question")
						&& m.Name == "generateQuestion"
						&& m.ParameterTypes.Count == 1
						&& m.ParameterTypes.Single() == "int"))
			{
				return new QuestionGenerationResult
				(
					error: "The QuestionGenerator class must have a method with the following signature: \n"
						+ "public static Question generateQuestion(int seed)" 
				);
			}

			if (!classJobResult.TestsCompilationResult.Success)
			{
				return new QuestionGenerationResult
				(
					error: "Failed to compile question generator. Compilation errors: \n"
						+ string.Join(
							"\n",
							classJobResult.TestsCompilationResult.Errors.Select(error => error.FullError))
				);
			}

			if (!classJobResult.TestResults[0].Completed)
			{
				return new QuestionGenerationResult
				(
					error: "An exception was encountered when calling QuestionGenerator.generateQuestion: \n"
						+ classJobResult.TestResults[0].Exception
				);
			}

			return new QuestionGenerationResult
			(
				classJobResult.TestResults[0].ReturnValue,
				classJob.FileContents,
				classJob.LineNumberOffset,
				seed
			);
		}

		/// <summary>
		/// Creates a class job that will generate the question, and return the result
		/// in the form of a json string.
		/// </summary>
		private ClassJob CreateQuestionGeneratorClassJob(
			GeneratedQuestionTemplate question,
			int seed)
		{
			var classesToImport = GetClassesToImport(question);
			int lineOffset = question.FullGeneratorFileLineOffset;

			return new ClassJob()
			{
				ClassesToImport = classesToImport,
				ClassName = "QuestionGenerator",
				FileContents = !string.IsNullOrEmpty(question.FullGeneratorFileContents)
					? question.FullGeneratorFileContents
					: GetGeneratorFile(question.GeneratorContents, out lineOffset),
				LineNumberOffset = lineOffset,
				Tests = new List<ClassTest>()
				{
					new ClassTest()
					{
						TestName = "QuestionGenerator",
						MethodBody = string.Join(
							"\n",
							new List<string>()
							{
								"ObjectMapper mapper = new ObjectMapper();",
								$"return mapper.writeValueAsString(QuestionGenerator.generateQuestion({seed}));"
							}),
						ReturnType = "String"
					}
				}
			};
		}

		/// <summary>
		/// Returns a list of classes to import.
		/// </summary>
		private List<string> GetClassesToImport(GeneratedQuestionTemplate question)
		{
			return new List<string>()
			{
				"com.fasterxml.jackson.databind.ObjectMapper",
				"com.fasterxml.jackson.databind.JsonMappingException"
			}.Concat
			(
				question.ImportedClasses?.Select
				(
					importedClass => importedClass.ClassName
				) ?? Enumerable.Empty<string>()
			).ToList();
		}

		/// <summary>
		/// Returns the contents of the file used to generate the question.
		/// </summary>
		private string GetGeneratorFile(string generatorContents, out int lineOffset)
		{
			var fileBuilder = new JavaFileBuilder();
			var javaModel = _questionModelFactory.GetQuestionModel();

			foreach (var javaClass in javaModel)
			{
				var javaClassDefinitionGenerator = _javaCodeGenerationFactory
					.CreateJavaClassDefinitionGenerator(fileBuilder, javaClass);

				javaClassDefinitionGenerator.GenerateClassDefinition();
				fileBuilder.AddBlankLine();
			}

			lineOffset = -fileBuilder.LinesAdded;
			fileBuilder.AddLines(generatorContents);

			return fileBuilder.GetFileContents().Replace("%", "%%");
		}
	}
}
