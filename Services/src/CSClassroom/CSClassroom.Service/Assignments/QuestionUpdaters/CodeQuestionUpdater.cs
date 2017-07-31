using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionUpdaters
{
	/// <summary>
	/// Updates a code question.
	/// </summary>
	public abstract class CodeQuestionUpdater<TQuestion> : QuestionUpdater<TQuestion> where TQuestion : CodeQuestion
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		protected CodeQuestionUpdater(DatabaseContext dbContext, TQuestion question, IModelErrorCollection errors) 
			: base(dbContext, question, errors)
		{
		}

		/// <summary>
		/// Performs question-type-specific update operations.
		/// </summary>
		protected sealed override async Task UpdateQuestionImplAsync()
		{
			var tests = Question.GetTests().ToList();
			if (tests == null || tests.Count == 0)
			{
				Errors.AddError("Tests", "At least one test is required.");
				return;
			}

			UpdateTests(tests);
			UpdateCodeConstraints(Question.CodeConstraints);

			DbContext.RemoveUnwantedObjects
			(
				DbContext.ImportedClasses,
				importedClass => importedClass.Id,
				importedClass => importedClass.CodeQuestionId == Question.Id,
				Question.ImportedClasses
			);

			DbContext.RemoveUnwantedObjects
			(
				DbContext.CodeConstraints,
				constraint => constraint.Id,
				constraint => constraint.CodeQuestionId == Question.Id,
				Question.CodeConstraints
			);

			await UpdateCodeQuestionImplAsync();
		}

		/// <summary>
		/// Performs code-question-type-specific update operations.
		/// </summary>
		protected abstract Task UpdateCodeQuestionImplAsync();

		/// <summary>
		/// Updates each test.
		/// </summary>
		private static void UpdateTests(IEnumerable<CodeQuestionTest> tests)
		{
			int order = 0;
			foreach (var test in tests)
			{
				test.ExpectedOutput = test.ExpectedOutput?.Replace("\r\n", "\n");
				test.Order = order;
				order++;
			}
		}

		/// <summary>
		/// Updates each code constraint.
		/// </summary>
		private static void UpdateCodeConstraints(IEnumerable<CodeConstraint> constraints)
		{
			if (constraints != null)
			{
				int order = 0;
				foreach (var constraint in constraints)
				{
					constraint.Order = order;
					order++;
				}
			}
		}
	}
}
