using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults.Errors;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;

namespace CSC.CSClassroom.Service.Assignments.QuestionGraders
{
	/// <summary>
	/// Grades a class question.
	/// </summary>
	public class ClassQuestionGrader : CodeQuestionGrader<ClassQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassQuestionGrader(ClassQuestion question, ICodeRunnerService codeRunnerService) 
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

					ClassName = Question.ClassName,

					FileContents = GetFileTemplateWithSubmission(submission)
						.Replace("%", "%%"),

					LineNumberOffset = -GetLineNumberOffset(),

					Tests = Question.Tests.Select
					(
						test => new ClassTest()
						{
							TestName = GetCodeTestName(test),
							ReturnType = test.ReturnType,
							MethodBody = test.MethodBody
						}
					).ToList()
				}
			);
		}

		/// <summary>
		/// Returns the question file template, with the submission substituted for the placeholder.
		/// </summary>
		private string GetFileTemplateWithSubmission(CodeQuestionSubmission submission)
		{
			var templateLines = Question.FileTemplate.Split('\n');
			var submissionLine = templateLines.Single(line => line.Contains(ClassQuestion.SubmissionPlaceholder));
			var linePrefix = submissionLine.Substring(0, submissionLine.IndexOf(ClassQuestion.SubmissionPlaceholder));

			var submissionLines = submission.Contents.Split('\n');
			var indentedSubmission = string.Join("\n", submissionLines.Select(line => $"{linePrefix}{line}"));

			return Question.FileTemplate.Replace(submissionLine, indentedSubmission);
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

			if (!Question.AllowPublicFields && submittedClass.Fields.Any(field => field.IsPublic))
			{
				yield return new FieldVisibilityError(Question.ClassName);
			}

			if (Question.RequiredMethods != null)
			{
				foreach (var requiredMethodGroup in Question.RequiredMethods.GroupBy(rm => rm.Name))
				{
					var errors = GetMethodDefinitionErrors(requiredMethodGroup, submittedClass);

					foreach (var error in errors)
					{
						yield return error;
					}					
				}
			}
		}

		/// <summary>
		/// Returns any method definition errors.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetMethodDefinitionErrors(
			IGrouping<string, RequiredMethod> requiredMethods,
			ClassDefinition submittedClass)
		{
			var submittedOverloads = submittedClass.Methods
				.Where(m => m.Name == requiredMethods.Key);

			if (requiredMethods.Count() != submittedOverloads.Count())
			{
				yield return new MethodCountError
				(
					Question.ClassName,
					requiredMethods.Key,
					requiredMethods.First().IsStatic,
					requiredMethods.Count()
				);

				yield break;
			}

			foreach (var requiredMethod in requiredMethods)
			{
				var matchingSubmittedMethod = submittedClass.Methods.FirstOrDefault
				(
					submittedMethod => 
						   submittedMethod.Name == requiredMethod.Name
						&& submittedMethod.IsPublic == requiredMethod.IsPublic
						&& submittedMethod.IsStatic == requiredMethod.IsStatic
						&& submittedMethod.ReturnType == StripGenericsFromType(requiredMethod.ReturnType)
						&& submittedMethod.ParameterTypes.SequenceEqual
							(
								requiredMethod.ParamTypeList.Select(StripGenericsFromType)
							)
				);

				if (matchingSubmittedMethod == null)
				{
					yield return new MethodDefinitionError
					(
						submittedClass.Name,
						requiredMethod.Name,
						requiredMethod.IsStatic,
						requiredMethod.IsPublic,
						requiredMethod.ParamTypes,
						requiredMethod.ReturnType
					);
				}
			}
		}

		/// <summary>
		/// Returns the description of a code test.
		/// </summary>
		protected override string GetTestDescription(CodeQuestionTest test)
		{
			ClassQuestionTest classQuestionTest = test as ClassQuestionTest;
			if (classQuestionTest == null)
				throw new ArgumentException("Invalid test type", nameof(test));

			return classQuestionTest.Description;
		}

		/// <summary>
		/// Returns the line number offset for the code job.
		/// </summary>
		private int GetLineNumberOffset()
		{
			int lineNumberOffset = 0;
			foreach (string str in Question.FileTemplate.Split('\n'))
			{
				if (str.Contains(ClassQuestion.SubmissionPlaceholder))
					return lineNumberOffset;

				lineNumberOffset++;
			}

			throw new InvalidOperationException("File template does not contain submission placeholder text.");
		}

		/// <summary>
		/// Adds a class to a java file to run in the visualizer, for the given test.
		/// </summary>
		protected override void AddVisualizerClass(
			JavaFileBuilder builder,
			CodeQuestionTest test,
			CodeQuestionSubmission submission)
		{
			var classTest = test as ClassQuestionTest;
			if (test == null)
				throw new ArgumentException("Invalid test type", nameof(test));

			var templateWithSubmission = GetFileTemplateWithSubmission(submission);
			var regex = new Regex("public\\s+class");
			var modifiedSubmission = regex.Replace(templateWithSubmission, "class");

			builder.AddLines(modifiedSubmission);
			builder.AddBlankLine();
			builder.AddLine("public class Runner");
			builder.BeginScope("{");
				
				if (classTest.ReturnType != "void")
				{
					builder.AddLine($"public static {classTest.ReturnType} runTest()");
					builder.BeginScope("{");
					builder.AddLines(classTest.MethodBody);
					builder.EndScope("}");
				}

				builder.AddBlankLine();

				builder.AddLine("public static void main(String[] args)");
				builder.BeginScope("{");

				if (classTest.ReturnType != "void")
				{
					builder.AddLine($"{classTest.ReturnType} returnVal = runTest();");
				}
				else
				{
					builder.AddLines(classTest.MethodBody);
				}

				builder.EndScope("}");

			builder.EndScope("}");
		}
		
		/// <summary>
		/// Returns the command line arguments for the given test.
		/// </summary>
		protected override string[] GetVisualizerCommandLineArguments(CodeQuestionTest test)
		{
			return new string[0];
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
				return new MissingRequiredClassError(Question.ClassName);
			}

			return compilationError;
		}
	}
}
