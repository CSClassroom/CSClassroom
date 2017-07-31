using System;
using System.Collections.Generic;
using System.Text;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionResolvers
{
	/// <summary>
	/// The base class for classes that unit test different types
	/// of question resolvers.
	/// </summary>
	public class QuestionResolverUnitTestBase
	{
		/// <summary>
		/// Creates a new UserQuestionData object.
		/// </summary>
		protected UserQuestionData CreateUserQuestionData(
			bool attemptsRemaining,
			int templateQuestionId = 1,
			string cachedQuestionData = null,
			int? seed = null,
			Question question = null)
		{
			return new UserQuestionData()
			{
				AssignmentQuestion = new AssignmentQuestion()
				{
					Assignment = new Assignment()
					{
						MaxAttempts = attemptsRemaining
							? (int?)null
							: 1
					},
					QuestionId = templateQuestionId,
					Question = question
				},
				NumAttempts = 1,
				CachedQuestionData = cachedQuestionData,
				Seed = seed
			};
		}

		/// <summary>
		/// Creates a new UserQuestionSubmission object.
		/// </summary>
		protected UserQuestionSubmission CreateUserQuestionSubmission(
			string cachedQuestionData = null,
			int? seed = null)
		{
			return new UserQuestionSubmission()
			{
				CachedQuestionData = cachedQuestionData,
				Seed = seed
			};
		}
	}
}
