using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults.Errors;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;

namespace CSC.CSClassroom.Service.Assignments.QuestionGraders
{
	/// <summary>
	/// Grades a method question.
	/// </summary>
	public class MethodQuestionGrader : CodeQuestionGrader<MethodQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodQuestionGrader(MethodQuestion question, ICodeRunnerService codeRunnerService) 
			: base(question, codeRunnerService)
		{
		}

		/// <summary>
		/// Executes a code job to grade the question.
		/// </summary>
		protected override async Task<CodeJobResult> ExecuteJobAsync(CodeQuestionSubmission submission)
		{
			return await CodeRunnerService.ExecuteMethodJobAsync
			(
				new MethodJob()
				{
					ClassesToImport = Question.ImportedClasses
						?.Select(importedClass => importedClass.ClassName)
						?.ToList() ?? new List<string>(),

					MethodCode = submission.Contents.Replace("%", "%%"),

					Tests = Question.Tests.Select
					(
						test => new MethodTest()
						{
							TestName = GetCodeTestName(test),
							ParamValues = test.ParameterValues
						}
					).ToList()
				}
			);
		}

		/// <summary>
		/// Returns any errors with the submission's definition (classes/methods).
		/// </summary>
		protected override IEnumerable<CodeQuestionError> GetDefinitionErrors(
			CodeQuestionSubmission submission, 
			CodeJobResult jobResult)
		{
			var methodJobResult = jobResult as MethodJobResult;
			if (methodJobResult == null)
				throw new ArgumentException("Invalid job result type", nameof(jobResult));

			var submittedMethod = methodJobResult.MethodDefinition;
			if (jobResult.ClassCompilationResult.Success && submittedMethod == null)
			{
				yield return new MethodMissingError(null /*className*/, Question.MethodName, expectedStatic: true);

				yield break;
			}

			if (submittedMethod.Name != Question.MethodName)
			{
				yield return new MethodNameError(Question.MethodName, submittedMethod.Name);
			}

			if (!submittedMethod.IsPublic)
			{
				yield return new MethodVisibilityError(Question.MethodName, expectedPublic: true);
			}

			if (!submittedMethod.IsStatic)
			{
				yield return new MethodStaticError(Question.MethodName, expectedStatic: true);
			}

			if (submittedMethod.ReturnType != Question.ReturnType)
			{
				yield return new MethodReturnTypeError(Question.MethodName, Question.ReturnType, submittedMethod.ReturnType); 
			}

			var expectedParamTypes = Question.ParameterTypes
				.Split(',')
				.Select(paramType => paramType.Trim())
				.ToList();

			if (!expectedParamTypes.SequenceEqual(submittedMethod.ParameterTypes))
			{
				yield return new MethodParameterTypesError(Question.MethodName, expectedParamTypes, submittedMethod.ParameterTypes);
			}
		}

		/// <summary>
		/// Returns the description of a code test.
		/// </summary>
		protected override string GetTestDescription(CodeQuestionTest test)
		{
			MethodQuestionTest methodQuestionTest = test as MethodQuestionTest;
			if (methodQuestionTest == null)
				throw new ArgumentException("Invalid test type", nameof(test));

			return $"{Question.MethodName}({methodQuestionTest.ParameterValues})";
		}

		/// <summary>
		/// Adds a class to a java file to run in the visualizer, for the given test.
		/// </summary>
		protected override void AddVisualizerClass(
			JavaFileBuilder builder,
			CodeQuestionTest test,
			CodeQuestionSubmission submission)
		{
			var methodTest = test as MethodQuestionTest;
			if (test == null)
				throw new ArgumentException("Invalid test type", nameof(test));

			builder.AddLine("public class Exercise");
			builder.BeginScope("{");
				builder.AddLines(submission.Contents);
				builder.AddBlankLine();
				builder.AddLine("public static void main(String[] args)");
				builder.BeginScope("{");
					AddVisualizerMainMethodBody(builder, methodTest, submission);
				builder.EndScope("}");
			builder.EndScope("}");
		}

		/// <summary>
		/// Adds the body of the main method to the visualizer class.
		/// </summary>
		private void AddVisualizerMainMethodBody(
			JavaFileBuilder builder, 
			MethodQuestionTest methodTest, 
			CodeQuestionSubmission submission)
		{
			string variableDeclaration = Question.ReturnType == "void"
				? ""
				: $"{Question.ReturnType} returnValue = ";

			builder.AddLine($"{variableDeclaration}{Question.MethodName}({methodTest.ParameterValues});");
		}

		/// <summary>
		/// Returns the command line arguments for the given test.
		/// </summary>
		protected override string[] GetVisualizerCommandLineArguments(CodeQuestionTest test)
		{
			return new string[0];
		}
	}
}
