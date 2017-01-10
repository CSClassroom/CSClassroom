using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionUpdaters
{
	/// <summary>
	/// Unit tests for the QuestionUpdater base class.
	/// </summary>
	public class QuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that prerequisite questions are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_PrerequisiteQuestions()
		{
			var database = GetDatabase().Build();
			var questions = database.Context.ClassQuestions
				.Include(q => q.PrerequisiteQuestions)
				.ToList();

			database.Reload();

			questions[1].PrerequisiteQuestions.Clear();
			questions[1].PrerequisiteQuestions.Add
			(
				new PrerequisiteQuestion()
				{
					FirstQuestionId = questions[2].Id,
				}
			);
			questions[1].PrerequisiteQuestions.Add
			(
				new PrerequisiteQuestion()
				{
					FirstQuestionId = questions[3].Id,
				}
			);

			var errors = new MockErrorCollection();
			var updater = GetQuestionUpdater(database, questions[1], errors);
			await updater.UpdateQuestionAsync();
			database.Context.Questions.Update(questions[1]);
			database.Context.SaveChanges();

			database.Reload();
			var question = database.Context.ClassQuestions
				.Include(q => q.PrerequisiteQuestions)
				.Skip(1)
				.First();

			Assert.False(errors.HasErrors);
			Assert.Equal(2, question.PrerequisiteQuestions.Count);
			Assert.Equal(questions[2].Id, question.PrerequisiteQuestions[0].FirstQuestionId);
			Assert.Equal(0, question.PrerequisiteQuestions[0].Order);
			Assert.Equal(questions[3].Id, question.PrerequisiteQuestions[1].FirstQuestionId);
			Assert.Equal(1, question.PrerequisiteQuestions[1].Order);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new ClassQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", new ClassQuestion() { Name = "Question2" })
				.AddQuestion("Class1", "Category1", new ClassQuestion() { Name = "Question3" })
				.AddQuestion("Class1", "Category1", new ClassQuestion() { Name = "Question4" })
				.AddPrerequisiteQuestion("Class1", "Category1", "Question1", "Category1", "Question2");
		}

		/// <summary>
		/// Creates a new code question updater.
		/// </summary>
		private static QuestionUpdater<Question> GetQuestionUpdater(
			TestDatabase database,
			CodeQuestion question,
			IModelErrorCollection errors)
		{
			return new Mock<QuestionUpdater<Question>>(database.Context, question, errors).Object;
		}
	}
}
