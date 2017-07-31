using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;
using CSC.CSClassroom.Service.Questions.QuestionLoaders;
using CSC.CSClassroom.Service.Questions.QuestionUpdaters;
using CSC.CSClassroom.Service.Questions.Validators;
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
		/// Validates changes to questions.
		/// </summary>
		private readonly IQuestionValidator _questionValidator;

		/// <summary>
		/// Creates question loaders.
		/// </summary>
		private readonly IQuestionLoaderFactory _questionLoaderFactory;

		/// <summary>
		/// Creates question updaters.
		/// </summary>
		private readonly IQuestionUpdaterFactory _questionUpdaterFactory;

		/// <summary>
		/// Creates question loaders.
		/// </summary>
		private readonly IQuestionDuplicatorFactory _questionDuplicatorFactory;

		/// <summary>
		/// Generates questions using a seed.
		/// </summary>
		private readonly IQuestionGenerator _questionGenerator;

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionService(
			DatabaseContext dbContext,
			IQuestionValidator questionValidator,
			IQuestionLoaderFactory questionLoaderFactory, 
			IQuestionUpdaterFactory questionUpdaterFactory,
			IQuestionDuplicatorFactory questionDuplicatorFactory,
			IQuestionGenerator questionGenerator,
			IJsonSerializer jsonSerializer)
		{
			_dbContext = dbContext;
			_questionValidator = questionValidator;
			_questionLoaderFactory = questionLoaderFactory;
			_questionUpdaterFactory = questionUpdaterFactory;
			_questionDuplicatorFactory = questionDuplicatorFactory;
			_questionGenerator = questionGenerator;
			_jsonSerializer = jsonSerializer;
		}

		/// <summary>
		/// Returns the list of questions.
		/// </summary>
		public async Task<IList<Question>> GetQuestionsAsync(string classroomName)
		{
			return await _dbContext.Questions
				.Where(question => question.QuestionCategory.Classroom.Name == classroomName)
				.Where(question => question.QuestionCategory.RandomlySelectedQuestionId == null)
				.Include(question => question.QuestionCategory.Classroom)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the list of question choices for a randomly selected question.
		/// </summary>
		public async Task<QuestionCategory> GetQuestionChoicesAsync(string classroomName, int questionId)
		{
			return await _dbContext.QuestionCategories
				.Where(qc => qc.Classroom.Name == classroomName)
				.Where(qc => qc.RandomlySelectedQuestionId == questionId)
				.Include(qc => qc.Questions)
					.ThenInclude(q => q.QuestionCategory.Classroom)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns the question with the given id.
		/// </summary>
		public async Task<Question> GetQuestionAsync(string classroomName, int id)
		{
			var question = await _dbContext.Questions
				.Where(q => q.QuestionCategory.Classroom.Name == classroomName)
				.Include(q => q.QuestionCategory.Classroom)
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
		/// Returns the specific instance of a generated question template
		/// that corresponds to the given seed.
		/// </summary>
		public async Task<GeneratedQuestionInstance> GetQuestionInstanceAsync(
			string classroomName, 
			int id, 
			int seed)
		{
			var question = await GetQuestionAsync(classroomName, id);
			if (question == null)
			{
				return null;
			}

			var generatedQuestionTemplate = question as GeneratedQuestionTemplate;
			if (generatedQuestionTemplate == null)
				throw new InvalidOperationException("Seed can only be specified for a generated question.");

			var result = await _questionGenerator.GenerateQuestionAsync
			(
				generatedQuestionTemplate,
				seed
			);

			if (result.SerializedQuestion != null)
			{
				var resultingQuestion = _jsonSerializer.Deserialize<Question>(result.SerializedQuestion);

				resultingQuestion.Name = question.Name;
				resultingQuestion.QuestionCategoryId = question.QuestionCategoryId;
				resultingQuestion.QuestionCategory = question.QuestionCategory;

				return new GeneratedQuestionInstance(resultingQuestion, seed, error: null);
			}
			else
			{
				return new GeneratedQuestionInstance(null /*question*/, seed, result.Error);
			}
		}
		
		/// <summary>
		/// Creates a question.
		/// </summary>
		public async Task<bool> CreateQuestionAsync(
			string classroomName,
			Question question,
			IModelErrorCollection errors)
		{
			if (!await ValidateAndUpdateQuestionAsync(classroomName, question, errors))
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

			if (existingQuestion.IsQuestionTemplate)
			{
				throw new InvalidOperationException(
					"Cannot generate question from existing generated question.");
			}

			if (existingQuestion.HasChoices)
			{
				throw new InvalidOperationException(
					"Cannot generate question from existing randomly selected question.");
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
			if (!await ValidateAndUpdateQuestionAsync(classroomName, question, errors))
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
		public async Task<QuestionCategory> DeleteQuestionAsync(string classroomName, int id)
		{
			var question = await GetQuestionAsync(classroomName, id);

			_dbContext.Questions.Remove(question);
			await _dbContext.SaveChangesAsync();

			return question.QuestionCategory;
		}

		/// <summary>
		/// Validates that the question may be updated, and executes
		/// the corresponding question updater.
		/// </summary>
		private async Task<bool> ValidateAndUpdateQuestionAsync(
			string classroomName, 
			Question question, 
			IModelErrorCollection errors)
		{
			var isValid = await _questionValidator.ValidateQuestionAsync
			(
				question,
				errors,
				classroomName
			);

			if (!isValid)
			{
				return false;
			}

			await _questionUpdaterFactory
				.CreateQuestionUpdater(question, errors)
				.UpdateQuestionAsync();

			if (errors.HasErrors)
			{
				return false;
			}

			return true;
		}
	}
}
