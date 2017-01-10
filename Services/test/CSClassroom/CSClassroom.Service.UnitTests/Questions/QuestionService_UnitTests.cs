using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Questions.ServiceResults.Errors;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions;
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;
using CSC.CSClassroom.Service.Questions.QuestionGraders;
using CSC.CSClassroom.Service.Questions.QuestionLoaders;
using CSC.CSClassroom.Service.Questions.QuestionUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions
{
	/// <summary>
	/// Unit tests for the question service.
	/// </summary>
	public class QuestionService_UnitTests
	{
		/// <summary>
		/// Ensures that GetQuestionsAsync returns only questions
		/// for a given classroom.
		/// </summary>
		[Fact]
		public async Task GetQuestionsAsync_OnlyForClassroom()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestionCategory("Class1", "Category2")
				.AddQuestionCategory("Class2", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category2", new ClassQuestion() { Name = "Question2" })
				.AddQuestion("Class2", "Category1", new ClassQuestion() { Name = "Question3" })
				.Build();

			var questionService = CreateQuestionService(database.Context);
			var questions = await questionService.GetQuestionsAsync("Class1");
			var orderedQuestions = questions.OrderBy(q => q.QuestionCategory.Name)
				.ToList();

			Assert.Equal(2, orderedQuestions.Count);
			Assert.Equal("Class1", orderedQuestions[0].QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", orderedQuestions[0].QuestionCategory.Name);
			Assert.Equal("Question1", orderedQuestions[0].Name);
			Assert.Equal("Class1", orderedQuestions[1].QuestionCategory.Classroom.Name);
			Assert.Equal("Category2", orderedQuestions[1].QuestionCategory.Name);
			Assert.Equal("Question2", orderedQuestions[1].Name);
		}

		/// <summary>
		/// Ensures that GetQuestionAsync returns the desired question, 
		/// if it exists.
		/// </summary>
		[Fact]
		public async Task GetQuestionAsync_Exists_ReturnQuestion()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion
				(
					"Class1", 
					"Category1", 
					new MethodQuestion() { Name = "Question1" }
				).Build();

			var questionId = database.Context.Questions.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionService = CreateQuestionService
			(
				database.Context, 
				questionLoaderFactory: loaderFactory.Object
			);

			var question = await questionService.GetQuestionAsync
			(
				"Class1",
				questionId
			);

			loaderFactory.Verify(LoadQuestionExpression);
			Assert.Equal("Class1", question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", question.QuestionCategory.Name);
			Assert.Equal("Question1", question.Name);
		}

		/// <summary>
		/// Ensures that GetQuestionAsync returns null, if the desired
		/// question doesn't exist.
		/// </summary>
		[Fact]
		public async Task GetQuestionAsync_DoesntExist_ReturnNull()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var questionService = CreateQuestionService(database.Context);
			var question = await questionService.GetQuestionAsync
			(
				"Class1",
				id: 1
			);

			Assert.Null(question);
		}

		/// <summary>
		/// Ensures that CreateQuestionAsync actually creates the question, 
		/// when there is no existing question with the same name.
		/// </summary>
		[Fact]
		public async Task CreateQuestionAsync_NameCollision_QuestionNotCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.Build();

			var questionCategoryId = database.Context.QuestionCategories.First().Id;
			var modelErrors = new MockErrorCollection();
			var updaterFactory = GetMockQuestionUpdaterFactory();

			var questionService = CreateQuestionService
			(
				database.Context,
				questionUpdaterFactory: updaterFactory.Object
			);

			var result = await questionService.CreateQuestionAsync
			(
				"Class1",
				new MethodQuestion()
				{
					Name = "Question1",
					QuestionCategoryId = questionCategoryId
				},
				modelErrors
			);

			database.Reload();

			var numQuestions = database.Context.Questions.Count();

			Assert.False(result);
			Assert.True(modelErrors.HasErrors);
			Assert.True(modelErrors.VerifyErrors("Name"));
			Assert.Equal(1, numQuestions);
		}

		/// <summary>
		/// Ensures that CreateQuestionAsync actually creates the question.
		/// </summary>
		[Fact]
		public async Task CreateQuestionAsync_NoNameCollision_QuestionCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.Build();

			var questionCategoryId = database.Context.QuestionCategories.First().Id;
			var modelErrors = new Mock<IModelErrorCollection>();
			var updaterFactory = GetMockQuestionUpdaterFactory();

			var questionService = CreateQuestionService
			(
				database.Context, 
				questionUpdaterFactory: updaterFactory.Object
			);

			var result = await questionService.CreateQuestionAsync
			(
				"Class1",
				new MethodQuestion()
				{
					Name = "Question1",
					QuestionCategoryId = questionCategoryId
				}, 
				modelErrors.Object
			);

			database.Reload();

			var question = database.Context.Questions
				.Include(q => q.QuestionCategory.Classroom)
				.Single();

			Assert.True(result);
			Assert.Equal("Class1", question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", question.QuestionCategory.Name);
			Assert.Equal("Question1", question.Name);
			updaterFactory.Verify(UpdateQuestionExpression);
		}

		/// <summary>
		/// Ensures that UpdateQuestionAsync does not update the question,
		/// when there is another question with the same name as the updated name.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionCategoryAsync_NameCollision_QuestionNotUpdated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question2" })
				.Build();

			var modelErrors = new Mock<IModelErrorCollection>();
			var updaterFactory = GetMockQuestionUpdaterFactory();
			var question = database.Context.Questions
				.Single(q => q.Name == "Question2");

			var questionService = CreateQuestionService
			(
				database.Context,
				questionUpdaterFactory: updaterFactory.Object
			);

			// Update the category
			database.Context.Entry(question).State = EntityState.Detached;
			question.Name = "Question1";

			// Apply the update
			await questionService.UpdateQuestionAsync
			(
				"Class1",
				question,
				modelErrors.Object
			);

			database.Reload();

			var questionCounts = database.Context.Questions
				.GroupBy(q => q.Name)
				.ToDictionary(g => g.Key, g => g.Count());
			
			Assert.Equal(2, questionCounts.Count);
			Assert.Equal(1, questionCounts["Question1"]);
			Assert.Equal(1, questionCounts["Question2"]);
		}

		/// <summary>
		/// Ensures that UpdateQuestionAsync actually updates the question,
		/// when there is no question with the same name as the updated name.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionCategoryAsync_NoNameCollision_QuestionUpdated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1",  "Category1", 
					new MethodQuestion { Name = "Question1", AllowPartialCredit = false })
				.Build();
			
			var question = database.Context.Questions.First();
			var modelErrors = new Mock<IModelErrorCollection>();
			var updaterFactory = GetMockQuestionUpdaterFactory();

			var questionService = CreateQuestionService
			(
				database.Context,
				questionUpdaterFactory: updaterFactory.Object
			);

			// Update the category
			database.Context.Entry(question).State = EntityState.Detached;
			question.AllowPartialCredit = true;

			// Apply the update
			await questionService.UpdateQuestionAsync
			(
				"Class1",
				question,
				modelErrors.Object
			);

			database.Reload();

			question = database.Context.Questions
				.Include(qc => qc.QuestionCategory.Classroom)
				.Single();

			Assert.Equal("Class1", question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", question.QuestionCategory.Name);
			Assert.Equal("Question1", question.Name);
			Assert.Equal(true, question.AllowPartialCredit);
			updaterFactory.Verify(UpdateQuestionExpression);
		}

		/// <summary>
		/// Ensures that DeleteQuestionAsync actually deletes the question.
		/// </summary>
		[Fact]
		public async Task DeleteQuestionAsync_QuestionDeleted()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.Build();
			
			var questionId = database.Context.Questions.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object
			);

			await questionService.DeleteQuestionAsync
			(
				"Class1",
				questionId
			);

			database.Reload();

			Assert.Equal(0, database.Context.Questions.Count());
		}

		/// <summary>
		/// Ensures that we return the correct result for a question
		/// with no past submission.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_NormalQuestionNoPastSubmission_ReturnsQuestion()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.Build();

			var questionId = database.Context.Questions.First().Id;
			var userId = database.Context.Users.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object
			);

			var questionToSolve = await questionService.GetQuestionToSolveAsync
			(
				"Class1",
				userId,
				questionId
			);

			Assert.Equal("Class1", questionToSolve.Question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", questionToSolve.Question.QuestionCategory.Name);
			Assert.Equal("Question1", questionToSolve.Question.Name);
			Assert.Null(questionToSolve.LastSubmission);
		}

		/// <summary>
		/// Ensures that we return the correct result for a question
		/// with a past submission.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_NormalQuestionPastSubmission_ReturnsQuestionAndSubmission()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "PastSubmission1")
				.Build();

			var questionId = database.Context.Questions.First().Id;
			var userId = database.Context.Users.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();

			var serializer = new Mock<IJsonSerializer>();
			serializer
				.Setup(s => s.Deserialize<QuestionSubmission>("PastSubmission1"))
				.Returns(new CodeQuestionSubmission() { Contents = "PastSubmissionContents" });

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				jsonSerializer: serializer.Object
			);

			var questionToSolve = await questionService.GetQuestionToSolveAsync
			(
				"Class1",
				userId,
				questionId
			);

			Assert.Equal("Class1", questionToSolve.Question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", questionToSolve.Question.QuestionCategory.Name);
			Assert.Equal("Question1", questionToSolve.Question.Name);

			Assert.Equal
			(
				"PastSubmissionContents", 
				((CodeQuestionSubmission)questionToSolve.LastSubmission).Contents
			);
		}

		/// <summary>
		/// Ensures that we return the correct result for a  
		/// generated question with no past submission.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_GeneratedQuestionNoPastSubmission_ReturnsQuestion()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new GeneratedQuestionTemplate() { Name = "Question1" })
				.Build();

			var questionId = database.Context.Questions.First().Id;
			var userId = database.Context.Users.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var randomNumberProvider = GetMockRandomNumberProvider(randomNumber: 12345);
			var questionGenerator = GetMockQuestionGenerator(questionId);

			var jsonSerializer = new Mock<IJsonSerializer>();
			jsonSerializer
				.Setup(js => js.Deserialize<Question>("SerializedQuestion"))
				.Returns(new MethodQuestion());

			var timeProvider = new Mock<ITimeProvider>();
			timeProvider
				.Setup(tp => tp.UtcNow)
				.Returns(new DateTime(2016, 1, 1));

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				questionGenerator: questionGenerator.Object,
				jsonSerializer: jsonSerializer.Object,
				randomNumberProvider: randomNumberProvider.Object,
				timeProvider: timeProvider.Object
			);

			var questionToSolve = await questionService.GetQuestionToSolveAsync
			(
				"Class1",
				userId,
				questionId
			);

			Assert.Equal("Class1", questionToSolve.Question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", questionToSolve.Question.QuestionCategory.Name);
			Assert.Equal("Question1", questionToSolve.Question.Name);
			Assert.Null(questionToSolve.LastSubmission);
			Assert.True(questionToSolve.Question is MethodQuestion);

			database.Reload();
			var userQuestionData = database.Context.UserQuestionData.First();

			Assert.Equal("SerializedQuestion", userQuestionData.CachedQuestionData);
			Assert.Equal(new DateTime(2016, 1, 1), userQuestionData.CachedQuestionDataTime);
		}

		/// <summary>
		/// Ensures that we return the correct result for a question
		/// with a non-stale past submission (which does not require
		/// regenerating the question).
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_GeneratedQuestionNonStalePastSubmission_ReturnsCachedQuestionAndSubmission()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", 
					new GeneratedQuestionTemplate() { Name = "Question1", DateModified = new DateTime(2015, 12, 31)})
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "PastSubmission1",
					score: 0.0, dateSubmitted: new DateTime(2016, 1, 1), cachedQuestionData: "SerializedQuestion")
				.Build();

			var questionId = database.Context.Questions.First().Id;
			var userId = database.Context.Users.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();

			var serializer = new Mock<IJsonSerializer>();
			serializer
				.Setup(s => s.Deserialize<QuestionSubmission>("PastSubmission1"))
				.Returns(new CodeQuestionSubmission() { Contents = "PastSubmissionContents" });
			serializer
				.Setup(js => js.Deserialize<Question>("SerializedQuestion"))
				.Returns(new MethodQuestion());

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				jsonSerializer: serializer.Object
			);

			var questionToSolve = await questionService.GetQuestionToSolveAsync
			(
				"Class1",
				userId,
				questionId
			);

			Assert.Equal("Class1", questionToSolve.Question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", questionToSolve.Question.QuestionCategory.Name);
			Assert.Equal("Question1", questionToSolve.Question.Name);

			Assert.Equal
			(
				"PastSubmissionContents",
				((CodeQuestionSubmission)questionToSolve.LastSubmission).Contents
			);

			database.Reload();
			var userQuestionData = database.Context.UserQuestionData.First();

			Assert.Equal("SerializedQuestion", userQuestionData.CachedQuestionData);
			Assert.Equal(new DateTime(2016, 1, 1), userQuestionData.CachedQuestionDataTime);
		}

		/// <summary>
		/// Ensures that we return the correct result for a 
		/// generated question with a stale past submission 
		/// (which requires regenerating the question).
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_GeneratedQuestionStalePastSubmission_ReturnsRegeneratedQuestionAndSubmission()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1",
					new GeneratedQuestionTemplate() { Name = "Question1", DateModified = new DateTime(2016, 1, 2) })
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "PastSubmission1", score: 0.0, 
					dateSubmitted: new DateTime(2016, 1, 1), cachedQuestionData: "OldSerializedQuestion", seed: 12345)
				.Build();

			var questionId = database.Context.Questions.First().Id;
			var userId = database.Context.Users.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionGenerator = GetMockQuestionGenerator(questionId);
			var timeProvider = GetMockTimeProvider(new DateTime(2016, 1, 3));

			var serializer = new Mock<IJsonSerializer>();
			serializer
				.Setup(s => s.Deserialize<QuestionSubmission>("PastSubmission1"))
				.Returns(new CodeQuestionSubmission() { Contents = "PastSubmissionContents" });
			serializer
				.Setup(js => js.Deserialize<Question>("SerializedQuestion"))
				.Returns(new MethodQuestion());

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				questionGenerator: questionGenerator.Object,
				jsonSerializer: serializer.Object,
				timeProvider: timeProvider.Object
			);

			var questionToSolve = await questionService.GetQuestionToSolveAsync
			(
				"Class1",
				userId,
				questionId
			);

			Assert.Equal("Class1", questionToSolve.Question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", questionToSolve.Question.QuestionCategory.Name);
			Assert.Equal("Question1", questionToSolve.Question.Name);
			Assert.True(questionToSolve.Question is MethodQuestion);

			Assert.Equal
			(
				"PastSubmissionContents",
				((CodeQuestionSubmission)questionToSolve.LastSubmission).Contents
			);

			database.Reload();
			var userQuestionData = database.Context.UserQuestionData.First();

			Assert.Equal("SerializedQuestion", userQuestionData.CachedQuestionData);
			Assert.Equal(new DateTime(2016, 1, 3), userQuestionData.CachedQuestionDataTime);
		}

		/// <summary>
		/// Ensures that we return the question with a list of unsolved
		/// prerequisites, if any.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_UnsolvedPrerequisites_ReturnsQuestionAndPrerequisites()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question2" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question3" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question4" })
				.AddPrerequisiteQuestion("Class1", "Category1", "Question1", "Category1", "Question4")
				.AddPrerequisiteQuestion("Class1", "Category1", "Question2", "Category1", "Question4")
				.AddPrerequisiteQuestion("Class1", "Category1", "Question3", "Category1", "Question4")
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "Contents", score: 1.0)
				.AddQuestionSubmission("Class1", "Category1", "Question2", "User1", "Contents", score: 0.0)
				.Build();

			var questionId = database.Context.Questions
				.Single(q => q.Name == "Question4")
				.Id;

			var userId = database.Context.Users.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object
			);

			var questionToSolve = await questionService.GetQuestionToSolveAsync
			(
				"Class1",
				userId,
				questionId
			);
			
			Assert.Equal("Question4", questionToSolve.Question.Name);
			Assert.Equal(2, questionToSolve.UnsolvedPrerequisites.Count);
			Assert.Equal("Question2", questionToSolve.UnsolvedPrerequisites[0].Name);
			Assert.Equal("Question3", questionToSolve.UnsolvedPrerequisites[1].Name);
		}

		/// <summary>
		/// Ensures that DuplicateExistingQuestionAsync returns a
		/// duplicate of a given question.
		/// </summary>
		[Fact]
		public async Task DuplicateExistingQuestionAsync_ReturnDuplicate()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.Build();

			var questionId = database.Context.Questions.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var duplicatorFactory = GetMockQuestionDuplicatorFactory();
			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				questionDuplicatorFactory: duplicatorFactory.Object
			);

			var question = await questionService.DuplicateExistingQuestionAsync
			(
				"Class1",
				questionId
			);

			loaderFactory.Verify(LoadQuestionExpression);
			duplicatorFactory.Verify(DuplicateQuestionExpression);
			Assert.Equal("Question1 (duplicated)", question.Name);
		}


		/// <summary>
		/// Ensures that  GenerateFromExistingQuestionAsync returns a new generated
		/// question template, whose contents would generate the given question.
		/// </summary>
		[Fact]
		public async Task GenerateFromExistingQuestionAsync_ReturnsMatchingGeneratorTemplate()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.Build();

			var originalQuestion = database.Context.Questions.First();

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionGenerator = GetMockQuestionGenerator(originalQuestion.Id);
			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				questionGenerator: questionGenerator.Object
			);

			var generatedQuestion = (GeneratedQuestionTemplate)
				await questionService.GenerateFromExistingQuestionAsync
			(
				"Class1",
				originalQuestion.Id
			);
			
			loaderFactory.Verify(LoadQuestionExpression);
			Assert.Equal("Question1 (generated)", generatedQuestion.Name);
			Assert.Equal("Generated Question", generatedQuestion.Description);
			Assert.Equal(originalQuestion.QuestionCategoryId, generatedQuestion.QuestionCategoryId);
			Assert.Equal(1, generatedQuestion.ImportedClasses.Count);
			Assert.Equal("java.util.*", generatedQuestion.ImportedClasses[0].ClassName);

			var expectedContents =
				  "public class QuestionGenerator\n"
				+ "{\n" 
				+  "	public static Question generateQuestion(int seed)\n"
				+  "	{\n"
				+  "		QuestionConstructorInvocation\n"
				+  "	}\n"
				+ "}\n";

			Assert.Equal(expectedContents, generatedQuestion.GeneratorContents);
		}

		/// <summary>
		/// Ensures that we store and return the graded result of a submission.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NormalQuestion_StoresAndReturnsResult()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.Build();

			var questionId = database.Context.Questions.First().Id;
			var userId = database.Context.Users.First().Id;

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();
			var graderFactory = GetMockQuestionGraderFactory();
			var timeProvider = GetMockTimeProvider(new DateTime(2016, 1, 1));
			var questionSubmission = new CodeQuestionSubmission()
			{
				Contents = "SubmissionContents"
			};

			var serializer = new Mock<IJsonSerializer>();
			serializer
				.Setup(s => s.Serialize<QuestionSubmission>(questionSubmission))
				.Returns("SerializedSubmissionContents");

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				questionGraderFactory: graderFactory.Object,
				jsonSerializer: serializer.Object,
				timeProvider: timeProvider.Object
			);

			var result = await questionService.GradeSubmissionAsync
			(
				"Class1",
				userId,
				questionId,
				questionSubmission
			);

			Assert.Equal(1.0, result.Score);

			database.Reload();

			var submission = database.Context
				.UserQuestionSubmissions
				.Where(uqs => uqs.UserQuestionData.UserId == userId)
				.Include(uqs => uqs.UserQuestionData)
				.Single(uqs => uqs.UserQuestionData.QuestionId == questionId);

			Assert.Equal(1.0, submission.Score);

			Assert.Equal
			(
				"SerializedSubmissionContents", 
				submission.UserQuestionData.LastQuestionSubmission
			);

			Assert.Equal
			(
				new DateTime(2016, 1, 1),
				submission.DateSubmitted
			);
		}

		/// <summary>
		/// Returns a mock question loader factory.
		/// </summary>
		private Mock<QuestionLoaderFactory> GetMockQuestionLoaderFactory()
		{
			var loaderFactory = new Mock<QuestionLoaderFactory>(null /*databaseContext*/);

			loaderFactory
				.Setup(LoadQuestionExpression)
				.Returns(Task.CompletedTask);

			return loaderFactory;
		}

		/// <summary>
		/// Returns a mock question updater factory.
		/// </summary>
		private Mock<QuestionUpdaterFactory> GetMockQuestionUpdaterFactory()
		{
			var updaterFactory = new Mock<QuestionUpdaterFactory>
			(
				null /*databaseContext*/,
				null /*questionGenerator*/
			);

			updaterFactory
				.Setup(UpdateQuestionExpression)
				.Returns(Task.CompletedTask);

			return updaterFactory;
		}

		/// <summary>
		/// Returns a mock question updater factory.
		/// </summary>
		private Mock<QuestionDuplicatorFactory> GetMockQuestionDuplicatorFactory()
		{
			var duplicatorFactory = new Mock<QuestionDuplicatorFactory>(null /*databaseContext*/);

			duplicatorFactory
				.Setup(DuplicateQuestionExpression)
				.Returns(new MethodQuestion() { Name = "Question1" });

			return duplicatorFactory;
		}

		/// <summary>
		/// Returns a mock question grader factory.
		/// </summary>
		private Mock<QuestionGraderFactory> GetMockQuestionGraderFactory()
		{
			var scoredQuestionResult = new ScoredQuestionResult
			(
				new CodeQuestionResult
				(
					new List<CodeQuestionError>(),
					new List<CodeQuestionTestResult>()
				),
				1.0
			);

			var graderFactory = new Mock<QuestionGraderFactory>(null /*databaseContext*/);

			graderFactory
				.Setup(GradeQuestionExpression)
				.ReturnsAsync(scoredQuestionResult);

			return graderFactory;
		}

		/// <summary>
		/// Returns a mock random number provider.
		/// </summary>
		private static Mock<IRandomNumberProvider> GetMockRandomNumberProvider(
			int randomNumber)
		{
			var randomNumberProvider = new Mock<IRandomNumberProvider>();

			randomNumberProvider
				.Setup(rnp => rnp.NextInt())
				.Returns(randomNumber);

			return randomNumberProvider;
		}

		/// <summary>
		/// Returns a mock question generator.
		/// </summary>
		private static Mock<IQuestionGenerator> GetMockQuestionGenerator(int questionId)
		{
			var questionGenerator = new Mock<IQuestionGenerator>();

			questionGenerator
				.Setup
				(
					qg => qg.GenerateConstructorInvocation
					(
						It.Is<Question>(q => q.Id == questionId),
						It.IsAny<JavaFileBuilder>()
					)
				)
				.Callback<Question, JavaFileBuilder>
				(
					(question, builder) => builder.AddLine("QuestionConstructorInvocation")
				);

			questionGenerator
				.Setup
				(
					qg => qg.GenerateQuestionAsync
					(
						It.Is<GeneratedQuestionTemplate>(q => q.Id == questionId),
						12345 /*seed*/
					)
				)
				.ReturnsAsync
				(
					new QuestionGenerationResult
					(
						"SerializedQuestion",
						fullGeneratorFileContents: null,
						fullGeneratorFileLineOffset: 0
					)
				);

			return questionGenerator;
		}

		/// <summary>
		/// Returns a mock time provider.
		/// </summary>
		private static Mock<ITimeProvider> GetMockTimeProvider(DateTime now)
		{
			var timeProvider = new Mock<ITimeProvider>();

			timeProvider
				.Setup(tp => tp.UtcNow)
				.Returns(now);

			return timeProvider;
		}

		/// <summary>
		/// Returns the expression to load a question.
		/// </summary>
		private Expression<Func<QuestionLoaderFactory, Task>> LoadQuestionExpression =>
			loaderFactory => loaderFactory
				.CreateQuestionLoader(It.IsNotNull<Question>())
				.LoadQuestionAsync();

		/// <summary>
		/// Returns the expression to update a question.
		/// </summary>
		private Expression<Func<QuestionUpdaterFactory, Task>> UpdateQuestionExpression =>
			updaterFactory => updaterFactory
				.CreateQuestionUpdater
				(
					It.IsNotNull<Question>(), 
					It.IsNotNull<IModelErrorCollection>()
				).UpdateQuestionAsync();

		/// <summary>
		/// Returns the expression to duplicate a question.
		/// </summary>
		private Expression<Func<QuestionDuplicatorFactory, Question>> DuplicateQuestionExpression =>
			duplicatorFactory => duplicatorFactory
				.CreateQuestionDuplicator(It.IsNotNull<Question>())
				.DuplicateQuestion();

		/// <summary>
		/// Returns the expression to duplicate a question.
		/// </summary>
		private Expression<Func<QuestionGraderFactory, Task<ScoredQuestionResult>>> GradeQuestionExpression =>
			graderFactory => graderFactory
				.CreateQuestionGrader(It.IsNotNull<Question>())
				.GradeSubmissionAsync(It.IsNotNull<QuestionSubmission>());

		/// <summary>
		/// Creates a question service.
		/// </summary>
		private QuestionService CreateQuestionService(
			DatabaseContext dbContext,
			QuestionLoaderFactory questionLoaderFactory = null,
			QuestionUpdaterFactory questionUpdaterFactory = null,
			QuestionGraderFactory questionGraderFactory = null,
			QuestionDuplicatorFactory questionDuplicatorFactory = null,
			IQuestionGenerator questionGenerator = null,
			IJsonSerializer jsonSerializer = null,
			IRandomNumberProvider randomNumberProvider = null,
			ITimeProvider timeProvider = null)
		{
			return new QuestionService
			(
				dbContext,
				questionLoaderFactory,
				questionUpdaterFactory,
				questionGraderFactory,
				questionDuplicatorFactory,
				questionGenerator,
				jsonSerializer,
				randomNumberProvider,
				timeProvider
			);
		}
	}
}
