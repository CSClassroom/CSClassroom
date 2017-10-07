using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Assignments.ServiceResults.Errors;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionDuplicators;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.Assignments.QuestionGraders;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.Assignments.QuestionUpdaters;
using CSC.CSClassroom.Service.Assignments.Validators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments
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
		/// Ensures that GetQuestionsAsync returns only questions
		/// for a given classroom.
		/// </summary>
		[Fact]
		public async Task GetQuestionChoicesAsync_ReturnsAllChoices()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new RandomlySelectedQuestion()
				{
					Name = "Question1",
					ChoicesCategory = new QuestionCategory()
					{
						Name = "ChoicesCategory",
						Questions = Collections.CreateList
						(
							new MethodQuestion() { Name = "Choice1" },
							new MethodQuestion() { Name = "Choice2" }
						).Cast<Question>().ToList()
					}
				})
				.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion()
				{
					Name = "OtherQuestion"
				})
				.Build();

			var questionId = database.Context
				.RandomlySelectedQuestions
				.Single()
				.Id;

			var questionService = CreateQuestionService(database.Context);
			var choicesCategory = await questionService.GetQuestionChoicesAsync
			(
				"Class1",
				questionId
			);

			var orderedQuestions = choicesCategory.Questions
				.OrderBy(q => q.Name)
				.ToList();

			Assert.Equal(2, orderedQuestions.Count);
			Assert.Equal("Class1", orderedQuestions[0].QuestionCategory.Classroom.Name);
			Assert.Equal("Choice1", orderedQuestions[0].Name);
			Assert.Equal("Class1", orderedQuestions[1].QuestionCategory.Classroom.Name);
			Assert.Equal("Choice2", orderedQuestions[1].Name);
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
		/// Ensures that GetQuestionInstanceAsync returns null when the given generated
		/// question template ID does not exist.
		/// </summary>
		[Fact]
		public async Task GetQuestionInstanceAsync_NoQuestion_ReturnsNull()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var questionService = CreateQuestionService(database.Context);
			var questionInstance = await questionService.GetQuestionInstanceAsync
			(
				"Class1",
				id: 1,
				seed: 100
			);

			Assert.Null(questionInstance);
		}

		/// <summary>
		/// Ensures that GetQuestionInstanceAsync throws when given a question that is
		/// not a generated question template.
		/// </summary>
		[Fact]
		public async Task GetQuestionInstanceAsync_InvalidQuestionType_Throws()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion()
				{
					Name = "Question1"
				})
				.Build();

			var questionId = database.Context
				.Questions
				.First()
				.Id;

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object
			);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await questionService.GetQuestionInstanceAsync
				(
					"Class1",
					id: questionId,
					seed: 100
				)
			);
		}

		/// <summary>
		/// Ensures that GetQuestionInstanceAsync returns the generated instance
		/// when there is no generation error.
		/// </summary>
		[Fact]
		public async Task GetQuestionInstanceAsync_NoGenerationError_ReturnsQuestionInstance()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new GeneratedQuestionTemplate()
				{
					Name = "Question1"
				})
				.Build();

			var questionId = database.Context
				.Questions
				.First()
				.Id;

			var loaderFactory = GetMockQuestionLoaderFactory();
			var generator = GetMockQuestionGenerator
			(
				questionId, 
				seed: 12345, 
				error: false,
				result: "SerializedQuestion"
			);

			var expectedResult = new MethodQuestion();
			var jsonSerializer = GetMockJsonSerializer<Question>
			(
				"SerializedQuestion", 
				expectedResult
			);

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				questionGenerator: generator.Object,
				jsonSerializer: jsonSerializer.Object	
			);

			var result = await questionService.GetQuestionInstanceAsync
			(
				"Class1",
				id: 1,
				seed: 12345
			);

			loaderFactory.Verify(LoadQuestionExpression);
			Assert.Equal(expectedResult, result.Question);
			Assert.Equal(12345, result.Seed);
		}

		/// <summary>
		/// Ensures that GetQuestionInstanceAsync returns the generated instance
		/// when there is no generation error.
		/// </summary>
		[Fact]
		public async Task GetQuestionInstanceAsync_GenerationError_ReturnsError()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new GeneratedQuestionTemplate()
				{
					Name = "Question1"
				})
				.Build();

			var questionId = database.Context
				.Questions
				.First()
				.Id;

			var loaderFactory = GetMockQuestionLoaderFactory();
			var questionGenerator = GetMockQuestionGenerator
			(
				questionId,
				seed: 12345,
				error: true,
				result: "Error"
			);

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object,
				questionGenerator: questionGenerator.Object
			);

			var result = await questionService.GetQuestionInstanceAsync
			(
				"Class1",
				id: 1,
				seed: 12345
			);

			Assert.Null(result.Question);
			Assert.Equal(12345, result.Seed);
			Assert.Equal("Error", result.Error);
		}

		/// <summary>
		/// Ensures that CreateQuestionAsync does not create the given question
		/// when the question is invalid.
		/// </summary>
		[Theory]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task CreateQuestionAsync_InvalidQuestion_QuestionNotCreated(
			bool validatorIsValid,
			bool updaterIsValid)
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.Build();

			var questionCategoryId = database.Context.QuestionCategories.First().Id;
			var modelErrors = new MockErrorCollection();
			var validator = GetMockQuestionValidator(validatorIsValid);
			var updaterFactory = GetMockQuestionUpdaterFactory(updaterIsValid);

			var questionService = CreateQuestionService
			(
				database.Context,
				questionValidator: validator.Object,
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

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Error"));

			database.Reload();

			var numQuestions = database.Context.Questions.Count();

			Assert.False(result);
			Assert.True(modelErrors.HasErrors);
			Assert.True(modelErrors.VerifyErrors("Name"));
			Assert.Equal(1, numQuestions);
		}

		/// <summary>
		/// Ensures that CreateQuestionAsync actually creates the question
		/// when the question is valid.
		/// </summary>
		[Fact]
		public async Task CreateQuestionAsync_ValidQuestion_QuestionCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.Build();

			var questionCategoryId = database.Context.QuestionCategories.First().Id;
			var modelErrors = new MockErrorCollection();
			var validator = GetMockQuestionValidator(isValid: true);
			var updaterFactory = GetMockQuestionUpdaterFactory(isValid: true);

			var questionService = CreateQuestionService
			(
				database.Context, 
				questionValidator: validator.Object,
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

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);

			database.Reload();

			var question = database.Context.Questions
				.Include(q => q.QuestionCategory.Classroom)
				.Single();

			Assert.True(result);
			Assert.Equal("Class1", question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", question.QuestionCategory.Name);
			Assert.Equal("Question1", question.Name);
		}

		/// <summary>
		/// Ensures that UpdateQuestionAsync does not update the question,
		/// when the question is invalid.
		/// </summary>
		[Theory]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task UpdateQuestionCategoryAsync_InvalidQuestion_QuestionNotUpdated(
			bool validatorIsValid,
			bool updaterIsValid)
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question2" })
				.Build();

			var modelErrors = new MockErrorCollection();
			var validator = GetMockQuestionValidator(validatorIsValid);
			var updaterFactory = GetMockQuestionUpdaterFactory(updaterIsValid);
			var question = database.Context.Questions
				.Single(q => q.Name == "Question2");

			database.Context.Entry(question).State = EntityState.Detached;
			question.AllowPartialCredit = true;

			var questionService = CreateQuestionService
			(
				database.Context,
				questionUpdaterFactory: updaterFactory.Object,
				questionValidator: validator.Object
			);
			
			var result = await questionService.UpdateQuestionAsync
			(
				"Class1",
				question,
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Error"));

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
		/// when the question is valid.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionCategoryAsync_ValidQuestion_QuestionUpdated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1",  "Category1", 
					new MethodQuestion { Name = "Question1", AllowPartialCredit = false })
				.Build();
			
			var question = database.Context.Questions.First();
			var modelErrors = new MockErrorCollection();
			var validator = GetMockQuestionValidator(isValid: true);
			var updaterFactory = GetMockQuestionUpdaterFactory(isValid: true);

			database.Context.Entry(question).State = EntityState.Detached;
			question.AllowPartialCredit = true;

			var questionService = CreateQuestionService
			(
				database.Context,
				questionValidator: validator.Object,
				questionUpdaterFactory: updaterFactory.Object
			);
			
			var result = await questionService.UpdateQuestionAsync
			(
				"Class1",
				question,
				modelErrors
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);

			database.Reload();

			question = database.Context.Questions
				.Include(qc => qc.QuestionCategory.Classroom)
				.Single();

			Assert.Equal("Class1", question.QuestionCategory.Classroom.Name);
			Assert.Equal("Category1", question.QuestionCategory.Name);
			Assert.Equal("Question1", question.Name);
			Assert.True(question.AllowPartialCredit);
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
		/// Ensures that GenerateFromExistingQuestionAsync throws if given a generated
		/// question template.
		/// </summary>
		[Fact]
		public async Task GenerateFromExistingQuestionAsync_FromGeneratedTemplate_Throws()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new GeneratedQuestionTemplate()
				{
					Name = "Question1"
				})
				.Build();

			var originalQuestion = database.Context.Questions.First();

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object
			);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await questionService.GenerateFromExistingQuestionAsync
				(
					"Class1",
					originalQuestion.Id
				)
			);
		}

		/// <summary>
		/// Ensures that GenerateFromExistingQuestionAsync throws if given a randomly
		/// selected question.
		/// </summary>
		[Fact]
		public async Task GenerateFromExistingQuestionAsync_FromRandomlySelectedQuestion_Throws()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new RandomlySelectedQuestion()
				{
					Name = "Question1"
				})
				.Build();

			var originalQuestion = database.Context.Questions.First();

			database.Reload();

			var loaderFactory = GetMockQuestionLoaderFactory();

			var questionService = CreateQuestionService
			(
				database.Context,
				questionLoaderFactory: loaderFactory.Object
			);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await questionService.GenerateFromExistingQuestionAsync
				(
					"Class1",
					originalQuestion.Id
				)
			);
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
			var questionGenerator = GetMockQuestionGenerator
			(
				originalQuestion.Id
			);

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
			Assert.Equal("Generated Question Template", generatedQuestion.Description);
			Assert.Equal(originalQuestion.QuestionCategoryId, generatedQuestion.QuestionCategoryId);
			Assert.Single(generatedQuestion.ImportedClasses);
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
		/// Returns a mock question validator.
		/// </summary>
		private Mock<IQuestionValidator> GetMockQuestionValidator(
			bool isValid)
		{
			var questionValidator = new Mock<IQuestionValidator>();

			questionValidator
				.Setup
				(
					m => m.ValidateQuestionAsync
					(
						It.IsNotNull<Question>(),
						It.IsNotNull<IModelErrorCollection>(),
						It.IsNotNull<string>()
					)
				).Callback<Question, IModelErrorCollection, string>
				(
					(_unused1, errors, _unused2) =>
					{
						if (!isValid)
						{
							errors.AddError("Error", "Error Description");
						}
					}
				).ReturnsAsync(isValid);

			return questionValidator;
		}

		/// <summary>
		/// Returns a mock question loader factory.
		/// </summary>
		private Mock<IQuestionLoaderFactory> GetMockQuestionLoaderFactory()
		{
			var loaderFactory = new Mock<IQuestionLoaderFactory>();

			loaderFactory
				.Setup(LoadQuestionExpression)
				.Returns(Task.CompletedTask);

			return loaderFactory;
		}

		/// <summary>
		/// Returns a mock question updater factory.
		/// </summary>
		private Mock<IQuestionUpdaterFactory> GetMockQuestionUpdaterFactory(
			bool isValid)
		{
			var updater = new Mock<IQuestionUpdater>();
			IModelErrorCollection errors = null;

			var updaterFactory = new Mock<IQuestionUpdaterFactory>();

			updaterFactory
				.Setup
				(
					m => m.CreateQuestionUpdater
					(
						It.IsNotNull<Question>(),
						It.IsNotNull<IModelErrorCollection>()
					)
				).Callback<Question, IModelErrorCollection>
				(
					(_unused, modelErrors) => errors = modelErrors
				).Returns(updater.Object);

			updater
				.Setup(m => m.UpdateQuestionAsync())
				.Callback
				(
					() =>
					{
						if (!isValid)
						{
							errors.AddError("Error", "ErrorDescription");
						}
					}
				).Returns(Task.CompletedTask);

			return updaterFactory;
		}

		/// <summary>
		/// Returns a mock question duplicator factory.
		/// </summary>
		private Mock<IQuestionDuplicatorFactory> GetMockQuestionDuplicatorFactory()
		{
			var duplicatorFactory = new Mock<IQuestionDuplicatorFactory>();

			duplicatorFactory
				.Setup(DuplicateQuestionExpression)
				.Returns(new MethodQuestion() { Name = "Question1" });

			return duplicatorFactory;
		}


		/// <summary>
		/// Returns a mock question generator.
		/// </summary>
		private static Mock<IQuestionGenerator> GetMockQuestionGenerator(
			int questionId,
			int seed = 0,
			bool error = false,
			string result = null)
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
						seed
					)
				)
				.ReturnsAsync
				(
					error
						? new QuestionGenerationResult(result)
						: new QuestionGenerationResult
							(
								result,
								fullGeneratorFileContents: null,
								fullGeneratorFileLineOffset: 0,
								seed: seed
							)
				);

			return questionGenerator;
		}

		/// <summary>
		/// Returns a mock JSON serializer.
		/// </summary>
		private Mock<IJsonSerializer> GetMockJsonSerializer<TObject>(
			string serializedObject, 
			TObject deserializedObject)
		{
			var serializer = new Mock<IJsonSerializer>();

			serializer
				.Setup(s => s.Deserialize<TObject>(serializedObject))
				.Returns(deserializedObject);

			return serializer;
		}

		/// <summary>
		/// Returns the expression to load a question.
		/// </summary>
		private Expression<Func<IQuestionLoaderFactory, Task>> LoadQuestionExpression =>
			loaderFactory => loaderFactory
				.CreateQuestionLoader(It.IsNotNull<Question>())
				.LoadQuestionAsync();

		/// <summary>
		/// Returns the expression to duplicate a question.
		/// </summary>
		private Expression<Func<IQuestionDuplicatorFactory, Question>> DuplicateQuestionExpression =>
			duplicatorFactory => duplicatorFactory
				.CreateQuestionDuplicator(It.IsNotNull<Question>())
				.DuplicateQuestion();

		/// <summary>
		/// Returns the expression to duplicate a question.
		/// </summary>
		private Expression<Func<IQuestionGraderFactory, Task<ScoredQuestionResult>>> GradeQuestionExpression =>
			graderFactory => graderFactory
				.CreateQuestionGrader(It.IsNotNull<Question>())
				.GradeSubmissionAsync(It.IsNotNull<QuestionSubmission>());

		/// <summary>
		/// Creates a question service.
		/// </summary>
		private QuestionService CreateQuestionService(
			DatabaseContext dbContext,
			IQuestionValidator questionValidator = null,
			IQuestionLoaderFactory questionLoaderFactory = null,
			IQuestionUpdaterFactory questionUpdaterFactory = null,
			IQuestionDuplicatorFactory questionDuplicatorFactory = null,
			IQuestionGenerator questionGenerator = null,
			IJsonSerializer jsonSerializer = null)
		{
			return new QuestionService
			(
				dbContext,
				questionValidator,
				questionLoaderFactory,
				questionUpdaterFactory,
				questionDuplicatorFactory,
				questionGenerator,
				jsonSerializer
			);
		}
	}
}
