using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Assignments;
using MoreLinq;

namespace CSC.CSClassroom.Service.Assignments.QuestionResolvers
{
	/// <summary>
	/// Resolves generated question templates.
	/// </summary>
	public class GeneratedQuestionTemplateResolver : IQuestionResolver
	{
		/// <summary>
		/// The question to resolve.
		/// </summary>
		private readonly UserQuestionData _userQuestionData;
		
		/// <summary>
		/// The json serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedQuestionTemplateResolver(
			UserQuestionData userQuestionData,
			IJsonSerializer jsonSerializer)
		{
			_jsonSerializer = jsonSerializer;
			_userQuestionData = userQuestionData;
		}

		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		public async Task<Question> ResolveUnsolvedQuestionAsync()
		{
			return await ResolveQuestionAsync(_userQuestionData.CachedQuestionData);
		}

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		public async Task<Question> ResolveSolvedQuestionAsync(
			UserQuestionSubmission submission)
		{
			return await ResolveQuestionAsync(submission.CachedQuestionData);
		}

		/// <summary>
		/// Resolves the given question.
		/// </summary>
		private Task<Question> ResolveQuestionAsync(string cachedQuestionData)
		{
			var actualQuestion = _jsonSerializer.Deserialize<Question>(cachedQuestionData);
			actualQuestion.Id = _userQuestionData.AssignmentQuestion.QuestionId;

			return Task.FromResult(actualQuestion);
		}
	}
}
