using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;
using CSC.CSClassroom.Service.Questions.QuestionGraders;
using CSC.CSClassroom.Service.Questions.QuestionLoaders;
using CSC.CSClassroom.Service.Questions.QuestionUpdaters;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions
{
	/// <summary>
	/// Performs question operations.
	/// </summary>
	public class QuestionService : IQuestionService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Creates question loaders.
		/// </summary>
		private readonly QuestionLoaderFactory _questionLoaderFactory;

		/// <summary>
		/// Creates question updaters.
		/// </summary>
		private readonly QuestionUpdaterFactory _questionUpdaterFactory;

		/// <summary>
		/// Creates question graders.
		/// </summary>
		private readonly QuestionGraderFactory _questionGraderFactory;

		/// <summary>
		/// Creates question loaders.
		/// </summary>
		private readonly QuestionDuplicatorFactory _questionDuplicatorFactory;

		/// <summary>
		/// Generates questions using a seed.
		/// </summary>
		private readonly IQuestionGenerator _questionGenerator;

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// A random number generator.
		/// </summary>
		private readonly IRandomNumberProvider _randomNumberProvider;

		/// <summary>
		/// Provides the current time.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionService(
			DatabaseContext dbContext, 
			QuestionLoaderFactory questionLoaderFactory, 
			QuestionUpdaterFactory questionUpdaterFactory,
			QuestionGraderFactory questionGraderFactory,
			QuestionDuplicatorFactory questionDuplicatorFactory,
			IQuestionGenerator questionGenerator,
			IJsonSerializer jsonSerializer,
			IRandomNumberProvider randomNumberProvider,
			ITimeProvider timeProvider)
		{
			_dbContext = dbContext;
			_questionLoaderFactory = questionLoaderFactory;
			_questionUpdaterFactory = questionUpdaterFactory;
			_questionGraderFactory = questionGraderFactory;
			_questionDuplicatorFactory = questionDuplicatorFactory;
			_questionGenerator = questionGenerator;
			_jsonSerializer = jsonSerializer;
			_randomNumberProvider = randomNumberProvider;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Returns the list of questions.
		/// </summary>
		public async Task<IList<Question>> GetQuestionsAsync(string classroomName)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			return await _dbContext.Questions
				.Where(question => question.QuestionCategory.ClassroomId == classroom.Id)
				.Include(question => question.QuestionCategory)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the question with the given id.
		/// </summary>
		public async Task<Question> GetQuestionAsync(string classroomName, int id)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			var question = await _dbContext.Questions
				.Where(q => q.QuestionCategory.ClassroomId == classroom.Id)
				.Include(q => q.QuestionCategory)
				.Include(q => q.PrerequisiteQuestions)
				.SingleOrDefaultAsync(q => q.Id == id);

			if (question == null)
			{
				return null;
			}
			
			await _questionLoaderFactory
				.CreateQuestionLoader(question)
				.LoadQuestionAsync();

			return question;
		}

		/// <summary>
		/// Returns the question with the given ID, along with any unsolved prerequisites
		/// for this question (if any), and the student's previous submission contents 
		/// (if any).
		/// </summary>
		public async Task<QuestionToSolve> GetQuestionToSolveAsync(
			string classroomName, 
			int userId, 
			int questionId)
		{
			var question = await GetQuestionAsync(classroomName, questionId);
			if (question == null)
				return null;

			var unsolvedPrereqs = await GetUnsolvedPrerequisitesAsync(userId, question);
			var userQuestionData = await EnsureUserQuestionDataAsync(userId, question);
			var lastSubmission = GetLastQuestionSubmission(userQuestionData);

			if (question is GeneratedQuestionTemplate)
			{
				var resultingQuestion = _jsonSerializer.Deserialize<Question>
				(
					userQuestionData.CachedQuestionData
				);

				resultingQuestion.Name = question.Name;
				resultingQuestion.QuestionCategoryId = question.QuestionCategoryId;
				resultingQuestion.QuestionCategory = question.QuestionCategory;

				return new QuestionToSolve(resultingQuestion, lastSubmission, unsolvedPrereqs);
			}
			else
			{
				return new QuestionToSolve(question, lastSubmission, unsolvedPrereqs);
			}
		}

		/// <summary>
		/// Returns the list of unsolved prerequisites for the given question.
		/// </summary>
		private async Task<IList<Question>> GetUnsolvedPrerequisitesAsync(
			int userId,
			Question question)
		{
			var solvedPrereqs = await _dbContext.UserQuestionData
				.Where(uqd => uqd.UserId == userId)
				.Where
				(
					uqd => uqd.Question
						.SubsequentQuestions
						.Any
						(
							sq => sq.SecondQuestionId == question.Id
						)
				)
				.Where(uqd => uqd.Submissions.Max(s => s.Score) == 1.0)
				.Select(uqd => uqd.QuestionId)
				.ToListAsync();

			var unsolvedPrereqs = question?.PrerequisiteQuestions
				?.Select(pq => pq.FirstQuestionId)
				?.Where(id => !solvedPrereqs.Contains(id))
				?.ToList() ?? new List<int>();

			return await _dbContext.Questions
				.Where(q => unsolvedPrereqs.Contains(q.Id))
				.ToListAsync();
		}

		/// <summary>
		/// Returns the last question submission, if any.
		/// </summary>
		private QuestionSubmission GetLastQuestionSubmission(
			UserQuestionData userQuestionData)
		{
			if (userQuestionData.LastQuestionSubmission == null)
				return null;

			try
			{
				return _jsonSerializer
					.Deserialize<QuestionSubmission>(userQuestionData.LastQuestionSubmission);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Ensures a user question data object exists for the given question
		/// (for the current user).
		/// </summary>
		private async Task<UserQuestionData> EnsureUserQuestionDataAsync(
			int userId,
			Question question)
		{
			var generatedQuestion = question as GeneratedQuestionTemplate;
			var userQuestionData = await _dbContext.UserQuestionData
				.SingleOrDefaultAsync
				(
					   s => s.UserId == userId
					&& s.QuestionId == question.Id
				);

			if (userQuestionData == null)
			{
				userQuestionData = new UserQuestionData()
				{
					UserId = userId,
					QuestionId = question.Id
				};

				if (generatedQuestion != null)
				{
					var seed = _randomNumberProvider.NextInt();
					await RegenerateQuestionAsync(generatedQuestion, seed, userQuestionData);
				}

				_dbContext.UserQuestionData.Add(userQuestionData);
			}
			else if (generatedQuestion != null 
				&& generatedQuestion.DateModified > userQuestionData.CachedQuestionDataTime)
			{
				await RegenerateQuestionAsync
				(
					generatedQuestion,
					userQuestionData.Seed ?? _randomNumberProvider.NextInt(),
					userQuestionData
				);
			}

			await _dbContext.SaveChangesAsync();

			return userQuestionData;
		}

		/// <summary>
		/// Regenerates the question.
		/// </summary>
		private async Task RegenerateQuestionAsync(
			GeneratedQuestionTemplate generatedQuestionTemplate,
			int seed,
			UserQuestionData userQuestionData)
		{
			var result = await _questionGenerator.GenerateQuestionAsync
			(
				generatedQuestionTemplate,
				seed
			);

			userQuestionData.Seed = seed;
			userQuestionData.CachedQuestionData = result.SerializedQuestion;
			userQuestionData.CachedQuestionDataTime = _timeProvider.UtcNow;
		}

		/// <summary>
		/// Creates a question.
		/// </summary>
		public async Task<bool> CreateQuestionAsync(
			string classroomName,
			Question question,
			IModelErrorCollection errors)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			var questionCategory = await _dbContext.QuestionCategories
				.SingleOrDefaultAsync(category => category.Id == question.QuestionCategoryId);

			if (questionCategory.ClassroomId != classroom.Id)
				throw new InvalidOperationException("Category of question is not in the given classroom.");

			if (await _dbContext.Questions.AnyAsync(q => 
				   q.Name == question.Name
				&& q.QuestionCategoryId == question.QuestionCategoryId))
			{
				errors.AddError("Name", "Another question with that name already exists.");
				return false;
			}

			await _questionUpdaterFactory.CreateQuestionUpdater(question, errors)
				.UpdateQuestionAsync();

			if (errors.HasErrors)
			{
				return false;
			}

			_dbContext.Add(question);
			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Returns a copy of an existing quesrtion. The copy is not saved unless and until
		/// it is submitted through CreateQuestionAsync.
		/// </summary>
		public async Task<Question> DuplicateExistingQuestionAsync(
			string classroomName,
			int existingQuestionId)
		{
			var existingQuestion = await GetQuestionAsync(classroomName, existingQuestionId);

			var duplicateQuestion = _questionDuplicatorFactory
				.CreateQuestionDuplicator(existingQuestion)
				.DuplicateQuestion();

			duplicateQuestion.Name = $"{duplicateQuestion.Name} (duplicated)";

			return duplicateQuestion;
		}

		/// <summary>
		/// Returns a new generated question template based off of an existing question. 
		/// The generated question is not saved unless and until it is submitted through 
		/// CreateQuestionAsync.
		/// </summary>
		public async Task<Question> GenerateFromExistingQuestionAsync(
			string classroomName,
			int existingQuestionId)
		{
			var existingQuestion = await GetQuestionAsync(classroomName, existingQuestionId);

			if (existingQuestion is GeneratedQuestionTemplate)
			{
				throw new InvalidOperationException(
					"Cannot generate question from existing generated question.");
			}

			var builder = new JavaFileBuilder();

			builder.AddLine("public class QuestionGenerator");
			builder.BeginScope("{");
				builder.AddLine("public static Question generateQuestion(int seed)");
				builder.BeginScope("{");
					_questionGenerator.GenerateConstructorInvocation
					(
						existingQuestion,
						builder
					);
				builder.EndScope("}");
			builder.EndScope("}");

			return new GeneratedQuestionTemplate()
			{
				Name = $"{existingQuestion.Name} (generated)",
				QuestionCategoryId = existingQuestion.QuestionCategoryId,
				Description = "Generated Question",
				ImportedClasses = new List<ImportedClass>()
				{
					new ImportedClass()
					{
						ClassName = "java.util.*"
					}
				},
				GeneratorContents = builder.GetFileContents()
			};
		}

		/// <summary>
		/// Updates a question.
		/// </summary>
		public async Task<bool> UpdateQuestionAsync(
			string classroomName,
			Question question, 
			IModelErrorCollection errors)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			var questionCategory = await _dbContext.QuestionCategories
				.SingleOrDefaultAsync
				(
					category => category.Id == question.QuestionCategoryId
				);

			if (questionCategory.ClassroomId != classroom.Id)
			{
				throw new InvalidOperationException(
					"Category of question is not in the given classroom.");
			}

			if (await _dbContext.Questions.AnyAsync(
				   q => q.Id != question.Id 
				&& q.Name == question.Name
				&& q.QuestionCategoryId == question.QuestionCategoryId))
			{
				errors.AddError("Name", "Another question with that name already exists.");
				return false;
			}

			await _questionUpdaterFactory.CreateQuestionUpdater(question, errors)
				.UpdateQuestionAsync();

			if (errors.HasErrors)
			{
				return false;
			}

			_dbContext.Update(question);
			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Removes a question.
		/// </summary>
		public async Task DeleteQuestionAsync(string classroomName, int id)
		{
			var question = await GetQuestionAsync(classroomName, id);

			_dbContext.Questions.Remove(question);
			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Grades a question submission (returning and storing the result).
		/// </summary>
		public async Task<ScoredQuestionResult> GradeSubmissionAsync(
			string classroomName,
			int userId,
			int questionId, 
			QuestionSubmission submission)
		{
			var question = await GetQuestionAsync(classroomName, questionId);
			var questionToSolve = await GetQuestionToSolveAsync(classroomName, userId, questionId);
			if (questionToSolve == null)
				return null;

			var scoredQuestionResult = await _questionGraderFactory
				.CreateQuestionGrader(questionToSolve.Question)
				.GradeSubmissionAsync(submission);

			var userQuestionData = await EnsureUserQuestionDataAsync(userId, question);

			if (userQuestionData.Submissions == null)
			{
				userQuestionData.Submissions = new List<UserQuestionSubmission>();
			}

			userQuestionData.Submissions.Add(new UserQuestionSubmission()
			{
				Score = scoredQuestionResult.Score,
				DateSubmitted = _timeProvider.UtcNow
			});

			userQuestionData.NumAttempts++;
			userQuestionData.LastQuestionSubmission = _jsonSerializer.Serialize(submission);

			await _dbContext.SaveChangesAsync();

			return scoredQuestionResult;
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		private async Task<Classroom> LoadClassroomAsync(string classroomName)
		{
			return await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.Include(c => c.Sections)
				.SingleOrDefaultAsync();
		}
	}
}
