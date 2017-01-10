using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults.Errors;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;

namespace CSC.CSClassroom.Service.Questions.QuestionGraders
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
					IEnumerable<CodeQuestionError> methodDefinitionErrors;

					if (requiredMethodGroup.Count() == 1)
					{
						var requiredMethod = requiredMethodGroup.First();

						methodDefinitionErrors = GetSingleMethodDefinitionErrors
						(
							requiredMethod, 
							submittedClass
						);
					}
					else
					{
						methodDefinitionErrors = GetOverloadedMethodDefinitionErrors
						(
							requiredMethodGroup,
							submittedClass
						);
					}

					foreach (var error in methodDefinitionErrors)
					{
						yield return error;
					}					
				}
			}
		}

		/// <summary>
		/// Returns method definition errors when a single method is expected.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetSingleMethodDefinitionErrors(
			RequiredMethod requiredMethod, 
			ClassDefinition submittedClass)
		{
			var submittedMethod = submittedClass.Methods
						.SingleOrDefault(method => method.Name == requiredMethod.Name);

			if (submittedMethod == null)
			{
				yield return new MethodMissingError
				(
					Question.ClassName,
					requiredMethod.Name,
					requiredMethod.IsStatic
				);

				yield break;
			}

			if (submittedMethod.IsPublic != requiredMethod.IsPublic)
			{
				yield return new MethodVisibilityError
				(
					requiredMethod.Name,
					requiredMethod.IsPublic
				);
			}

			if (submittedMethod.IsStatic != requiredMethod.IsStatic)
			{
				yield return new MethodStaticError
				(
					requiredMethod.Name,
					requiredMethod.IsStatic
				);
			}

			if (submittedMethod.ReturnType != requiredMethod.ReturnType)
			{
				yield return new MethodReturnTypeError
				(
					requiredMethod.Name,
					requiredMethod.ReturnType,
					submittedMethod.ReturnType
				);
			}

			if (!submittedMethod.ParameterTypes.SequenceEqual(requiredMethod.ParamTypeList))
			{
				yield return new MethodParameterTypesError
				(
					requiredMethod.Name,
					requiredMethod.ParamTypeList,
					submittedMethod.ParameterTypes
				);
			}
		}

		/// <summary>
		/// Returns method definition errors when a single method is expected.
		/// </summary>
		private IEnumerable<CodeQuestionError> GetOverloadedMethodDefinitionErrors(
			IGrouping<string, RequiredMethod> requiredOverloads,
			ClassDefinition submittedClass)
		{
			var submittedOverloads = submittedClass.Methods
				.Where(m => m.Name == requiredOverloads.Key);

			if (requiredOverloads.Count() != submittedOverloads.Count())
			{
				yield return new MethodOverloadCountError
				(
					Question.ClassName,
					requiredOverloads.Key,
					requiredOverloads.First().IsStatic,
					requiredOverloads.Count()
				);

				yield break;
			}

			foreach (var requiredMethod in requiredOverloads)
			{
				var submittedOverload = submittedClass.Methods.FirstOrDefault
				(
					submittedMethod => 
						   submittedMethod.IsPublic == requiredMethod.IsPublic
						&& submittedMethod.IsStatic == requiredMethod.IsStatic
						&& submittedMethod.ReturnType == requiredMethod.ReturnType
						&& submittedMethod.ParameterTypes.SequenceEqual(requiredMethod.ParamTypeList)
				);

				if (submittedOverload == null)
				{
					yield return new MethodOverloadDefinitionError
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
