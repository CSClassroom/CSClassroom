using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults.Errors;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;

namespace CSC.CSClassroom.Service.Questions.QuestionGraders
{
	/// <summary>
	/// Grades a program question.
	/// </summary>
	public class ProgramQuestionGrader : CodeQuestionGrader<ProgramQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ProgramQuestionGrader(ProgramQuestion question, ICodeRunnerService codeRunnerService) 
			: base(question, codeRunnerService)
		{
		}

		/// <summary>
		/// Executes a code job to grade the question.
		/// </summary>
		protected override async Task<CodeJobResult> ExecuteJobAsync(CodeQuestionSubmission submission)
		{
			return await CodeRunnerService.ExecuteClassJobAsync
			(
				new ClassJob()
				{
					ClassesToImport = Question.ImportedClasses
						?.Select(importedClass => importedClass.ClassName)
						?.ToList() ?? new List<string>(),

					ClassName = Question.ProgramClassName,

					FileContents = submission.Contents.Replace("%", "%%"),

					LineNumberOffset = 0,

					Tests = Question.Tests.Select
					(
						(test, index) => new ClassTest()
						{
							TestName = GetCodeTestName(test),
							ReturnType = "void",
							MethodBody = $"{Question.ProgramClassName}.main(new String[] {{{string.Join(",", GetCommandLineArgs(test, includeQuotes: true))}}});"
						}
					).ToList()
				}
			);
		}

		/// <summary>
		/// Returns a string containing the command line arguments separated by commas, 
		/// with each argument quoted.
		/// </summary>
		private string[] GetCommandLineArgs(ProgramQuestionTest test, bool includeQuotes)
		{
			return test.GetArguments().Select
			(
				arg => arg.StartsWith("\"")
					? includeQuotes ? arg : arg.Substring(1, arg.Length - 1)
					: includeQuotes ? $"\"{arg}\"" : arg
			).ToArray();
		}

		/// <summary>
		/// Returns any errors with the submission's definition (classes/methods).
		/// </summary>
		protected override IEnumerable<CodeQuestionError> GetDefinitionErrors(
			CodeQuestionSubmission submission,
			CodeJobResult jobResult)
		{
			var classJobResult = jobResult as ClassJobResult;
			if (classJobResult == null)
				throw new ArgumentException("Invalid job result type", nameof(jobResult));

			var submittedClass = classJobResult.ClassDefinition;
			if (submittedClass == null)
			{
				yield break;
			}

			var hasMainMethod = submittedClass.Methods.Any
			(
				method =>
					   method.IsPublic
					&& method.IsStatic
					&& method.Name == "main"
					&& method.ParameterTypes.Count == 1
					&& method.ParameterTypes[0] == "String[]"
					&& method.ReturnType == "void"
			);

			if (!hasMainMethod)
			{
				yield return new MainMethodMissingError(Question.ProgramClassName);
			}
		}

		/// <summary>
		/// Returns the description of a code test.
		/// </summary>
		protected override string GetTestDescription(CodeQuestionTest test)
		{
			ProgramQuestionTest programQuestionTest = test as ProgramQuestionTest;
			if (programQuestionTest == null)
				throw new ArgumentException("Invalid test type", nameof(test));

			return programQuestionTest.TestDescription;
		}

		/// <summary>
		/// Adds a class to a java file to run in the visualizer, for the given test.
		/// </summary>
		protected override void AddVisualizerClass(
			JavaFileBuilder builder,
			CodeQuestionTest test,
			CodeQuestionSubmission submission)
		{
			var classTest = test as ProgramQuestionTest;
			if (test == null)
				throw new ArgumentException("Invalid test type", nameof(test));

			builder.AddLines(submission.Contents);
		}
		
		/// <summary>
		/// Returns the command line arguments for the given test.
		/// </summary>
		protected override string[] GetVisualizerCommandLineArguments(CodeQuestionTest test)
		{
			ProgramQuestionTest programQuestionTest = test as ProgramQuestionTest;
			if (programQuestionTest == null)
				throw new ArgumentException("Invalid test type", nameof(test));

			return GetCommandLineArgs(programQuestionTest, includeQuotes: false);
		}

		/// <summary>
		/// Allows for returning a more friendly error, for compiler errors that
		/// aren't as friendly.
		/// </summary>
		protected override CodeQuestionError GetFriendlyError(
			ClassCompilationError compilationError)
		{
			if (compilationError.LineErrorText.Contains("should be declared in"))
			{
				return new MissingRequiredClassError(Question.ProgramClassName);
			}

			return compilationError;
		}
	}
}
