using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;
using CSC.CSClassroom.Service.Questions.QuestionUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionUpdaters
{
	/// <summary>
	/// Unit tests for the GeneratedQuestionUpdater class.
	/// </summary>
	public class GeneratedQuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that an error is returned if the template cannot
		/// generate a question.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_BrokenTemplate_ReturnsError()
		{
			var database = GetDatabase().Build();
			var question = database.Context.GeneratedQuestions.First();

			database.Reload();
			question.GeneratorContents = "NewGeneratedContents";

			var questionGenerator = GetMockQuestionGenerator
			(
				question,
				new QuestionGenerationResult("QuestionGenerationError")
			);

			var errors = new MockErrorCollection();
			var updater = new GeneratedQuestionUpdater
			(
				database.Context, 
				question, 
				errors, 
				questionGenerator.Object
			);

			await updater.UpdateQuestionAsync();

			Assert.True(errors.HasErrors);
			Assert.True(errors.VerifyErrors("GeneratorContents"));
		}

		/// <summary>
		/// Ensures that the full generator file is cached, if the template
		/// submitted is valid.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_ValidTemplate_UpdatesCachedContents()
		{
			var database = GetDatabase().Build();
			var question = database.Context.GeneratedQuestions.First();

			database.Reload();
			question.GeneratorContents = "NewGeneratedContents";

			var questionGenerator = GetMockQuestionGenerator
			(
				question,
				new QuestionGenerationResult
				(
					"SerializedQuestion",
					"FullGeneratorFileContents",
					fullGeneratorFileLineOffset: -10	
				)
			);

			var errors = new MockErrorCollection();
			var updater = new GeneratedQuestionUpdater
			(
				database.Context,
				question,
				errors,
				questionGenerator.Object
			);

			await updater.UpdateQuestionAsync();

			Assert.False(errors.HasErrors);
			Assert.Equal("FullGeneratorFileContents", question.FullGeneratorFileContents);
			Assert.Equal(-10, question.FullGeneratorFileLineOffset);
		}

		/// <summary>
		/// Returns a mock question generator.
		/// </summary>
		private Mock<IQuestionGenerator> GetMockQuestionGenerator(
			GeneratedQuestionTemplate question,
			QuestionGenerationResult generationResult)
		{
			var questionGenerator = new Mock<IQuestionGenerator>();

			questionGenerator
				.Setup(qg => qg.GenerateQuestionAsync(question, 0 /*seed*/))
				.ReturnsAsync(generationResult);

			return questionGenerator;
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new GeneratedQuestionTemplate() { Name = "Question1" });
		}
	}
}
