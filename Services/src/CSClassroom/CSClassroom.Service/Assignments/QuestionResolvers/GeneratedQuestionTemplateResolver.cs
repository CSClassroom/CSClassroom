using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Questions;
using MoreLinq;

namespace CSC.CSClassroom.Service.Questions.QuestionResolvers
{
	/// <summary>
	/// Resolves generated question templates.
	/// </summary>
	public class GeneratedQuestionTemplateResolver : QuestionResolver
	{
		/// <summary>
		/// The json serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedQuestionTemplateResolver(
			UserQuestionData userQuestionData,
			IJsonSerializer jsonSerializer) : base(userQuestionData)
		{
			_jsonSerializer = jsonSerializer;
		}

		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		protected override async Task<Question> ResolveUnsolvedQuestionImplAsync()
		{
			return await ResolveQuestionAsync(UserQuestionData.CachedQuestionData);
		}

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		public override async Task<Question> ResolveSolvedQuestionAsync(
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
			actualQuestion.Id = UserQuestionData.AssignmentQuestion.QuestionId;

			return Task.FromResult(actualQuestion);
		}
	}
}
