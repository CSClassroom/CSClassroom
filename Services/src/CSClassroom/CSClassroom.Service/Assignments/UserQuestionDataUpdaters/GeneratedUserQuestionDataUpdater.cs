using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using MoreLinq;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// A UserQuestionDataUpdater for generated question templates.
	/// </summary>
	public class GeneratedUserQuestionDataUpdater : IUserQuestionDataUpdater
	{
		/// <summary>
		/// The question generator.
		/// </summary>
		private readonly IQuestionGenerator _questionGenerator;

		/// <summary>
		/// The seed generator.
		/// </summary>
		private readonly IGeneratedQuestionSeedGenerator _seedGenerator;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// The regeneration jobs.
		/// </summary>
		private readonly IList<RegenerateQuestionJob> _regenerateJobs;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedUserQuestionDataUpdater(
			IQuestionGenerator questionGenerator,
			IGeneratedQuestionSeedGenerator seedGenerator, 
			ITimeProvider timeProvider)
		{
			_questionGenerator = questionGenerator;
			_seedGenerator = seedGenerator;
			_timeProvider = timeProvider;
			_regenerateJobs = new List<RegenerateQuestionJob>();
		}

		/// <summary>
		/// Adds a job to the batch that updates the given UserQuestionData object.
		/// </summary>
		public void AddToBatch(UserQuestionData userQuestionData)
		{
			if (!(userQuestionData.AssignmentQuestion.Question is GeneratedQuestionTemplate))
			{
				throw new InvalidOperationException("Invalid question type.");
			}

			if (IsNewSeedRequired(userQuestionData))
			{
				_regenerateJobs.Add
				(
					GetRegenerateJob(userQuestionData, seed: null)
				);
			}
			else if (WasQuestionModified(userQuestionData))
			{
				_regenerateJobs.Add
				(
					GetRegenerateJob(userQuestionData, userQuestionData.Seed)
				);
			}
		}

		/// <summary>
		/// Updates all UserQuestionData objects. 
		/// </summary>
		public async Task UpdateAllAsync()
		{
			var generationTime = _timeProvider.UtcNow;

			var regenerationTasks = _regenerateJobs
				.Select
				(
					job => _questionGenerator.GenerateQuestionAsync
					(
						job.GeneratedQuestionTemplate,
						job.Seed
					)
				).ToList();

			var results = await Task.WhenAll(regenerationTasks);

			for (int index = 0; index < results.Length; index++)
			{
				ApplyQuestionGenerationResult
				(
					_regenerateJobs[index].UserQuestionData, 
					results[index],
					generationTime
				);
			}
		}

		/// <summary>
		/// Returns whether or not the given question must be regenerated with a new seed.
		/// </summary>
		private bool IsNewSeedRequired(UserQuestionData userQuestionData)
		{
			return    userQuestionData.AnyAttemptsRemaining
				   && userQuestionData.Seed == null;
		}

		/// <summary>
		/// Returns whether or not the generated question template was modified since 
		/// the question was generated.
		/// </summary>
		private bool WasQuestionModified(UserQuestionData userQuestionData)
		{
			var generatedQuestion = (GeneratedQuestionTemplate)userQuestionData
				.AssignmentQuestion
				.Question;

			return generatedQuestion.DateModified > userQuestionData.CachedQuestionDataTime;
		}

		/// <summary>
		/// Regenerates the question.
		/// </summary>
		private RegenerateQuestionJob GetRegenerateJob(
			UserQuestionData userQuestionData,
			int? seed)
		{
			var generatedQuestionTemplate = (GeneratedQuestionTemplate)
				userQuestionData.AssignmentQuestion.Question;

			var maxSeed = generatedQuestionTemplate.NumSeeds ?? int.MaxValue;
			if (!seed.HasValue || seed.Value >= maxSeed)
			{
				seed = _seedGenerator.GenerateSeed(userQuestionData, maxSeed);
			}

			return new RegenerateQuestionJob(userQuestionData, seed.Value);
		}

		/// <summary>
		/// Applies a new generated question instance to a given question.
		/// </summary>
		private void ApplyQuestionGenerationResult(
			UserQuestionData userQuestionData,
			QuestionGenerationResult result,
			DateTime generationTime)
		{
			if (result.Error != null)
				throw new InvalidOperationException("Failed to regenerate question.");

			if (userQuestionData.Seed != result.Seed)
			{
				userQuestionData.LastQuestionSubmission = null;
			}

			userQuestionData.Seed = result.Seed;
			userQuestionData.CachedQuestionData = result.SerializedQuestion;
			userQuestionData.CachedQuestionDataTime = generationTime;
		}

		/// <summary>
		/// A job to regenerate a question.
		/// </summary>
		private class RegenerateQuestionJob
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			public RegenerateQuestionJob(UserQuestionData userQuestionData, int seed)
			{
				UserQuestionData = userQuestionData;
				Seed = seed;
			}

			/// <summary>
			/// The UserQuestionData object for the question to regenerate.
			/// </summary>
			public UserQuestionData UserQuestionData { get; }

			/// <summary>
			/// The seed to use when regenerating.
			/// </summary>
			public int Seed { get; }

			/// <summary>
			/// The actual generated question template for the given UserQuestionData.
			/// </summary>
			public GeneratedQuestionTemplate GeneratedQuestionTemplate =>
				(GeneratedQuestionTemplate) UserQuestionData.AssignmentQuestion.Question;
		}
	}
}
