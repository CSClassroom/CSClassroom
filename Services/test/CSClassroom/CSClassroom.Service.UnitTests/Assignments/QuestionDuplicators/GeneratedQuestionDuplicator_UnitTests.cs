using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionDuplicators
{
	/// <summary>
	/// Unit tests for the GeneratedQuestionDuplicator class.
	/// </summary>
	public class GeneratedQuestionDuplicator_UnitTests
	{
		/// <summary>
		/// Ensures that the duplication of a generated question returns
		/// a non-null duplicate.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_Succeeds()
		{
			var database = GetDatabase().Build();
			var question = database.Context.GeneratedQuestions.First();
			var duplicator = new GeneratedQuestionDuplicator(database.Context, question);
			var result = (GeneratedQuestionTemplate)duplicator.DuplicateQuestion();

			Assert.NotNull(result);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new GeneratedQuestionTemplate());
		}
	}
}
