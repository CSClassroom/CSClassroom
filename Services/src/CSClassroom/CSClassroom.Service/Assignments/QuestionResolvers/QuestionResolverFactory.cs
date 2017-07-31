using System;
using System.Collections.Generic;
using System.Text;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters;

namespace CSC.CSClassroom.Service.Assignments.QuestionResolvers
{
	/// <summary>
	/// Creates a question resolver for a given UserQuestionData object.
	/// </summary>
	public class QuestionResolverFactory : 
		IQuestionResolverFactory, 
		IQuestionResultVisitor<IQuestionResolver, UserQuestionData>
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The json serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// The question loader factory.
		/// </summary>
		private readonly IQuestionLoaderFactory _questionLoaderFactory;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionResolverFactory(
			DatabaseContext dbContext,
			IJsonSerializer jsonSerializer,
			IQuestionLoaderFactory questionLoaderFactory)
		{
			_dbContext = dbContext;
			_jsonSerializer = jsonSerializer;
			_questionLoaderFactory = questionLoaderFactory;
		}

		/// <summary>
		/// Creates a question resolver for a given UserQuestionData object.
		/// </summary>
		public IQuestionResolver CreateQuestionResolver(UserQuestionData userQuestionData)
		{
			return userQuestionData.AssignmentQuestion
				.Question
				.Accept(this, userQuestionData);
		}

		/// <summary>
		/// Creates a question loader for a multiple choice question.
		/// </summary>
		IQuestionResolver IQuestionResultVisitor<IQuestionResolver, UserQuestionData>.Visit(
			MultipleChoiceQuestion question,
			UserQuestionData userQuestionData)
		{
			return new DefaultQuestionResolver(userQuestionData);
		}

		/// <summary>
		/// Creates a question loader for a short answer question.
		/// </summary>
		IQuestionResolver IQuestionResultVisitor<IQuestionResolver, UserQuestionData>.Visit(
			ShortAnswerQuestion question,
			UserQuestionData userQuestionData)
		{
			return new DefaultQuestionResolver(userQuestionData);
		}

		/// <summary>
		/// Creates a question loader for a method question.
		/// </summary>
		IQuestionResolver IQuestionResultVisitor<IQuestionResolver, UserQuestionData>.Visit(
			MethodQuestion question,
			UserQuestionData userQuestionData)
		{
			return new DefaultQuestionResolver(userQuestionData);
		}

		/// <summary>
		/// Creates a question loader for a class question.
		/// </summary>
		IQuestionResolver IQuestionResultVisitor<IQuestionResolver, UserQuestionData>.Visit(
			ClassQuestion question,
			UserQuestionData userQuestionData)
		{
			return new DefaultQuestionResolver(userQuestionData);
		}

		/// <summary>
		/// Creates a question loader for a program question.
		/// </summary>
		IQuestionResolver IQuestionResultVisitor<IQuestionResolver, UserQuestionData>.Visit(
			ProgramQuestion question,
			UserQuestionData userQuestionData)
		{
			return new DefaultQuestionResolver(userQuestionData);
		}

		/// <summary>
		/// Creates a question loader for a generated question.
		/// </summary>
		IQuestionResolver IQuestionResultVisitor<IQuestionResolver, UserQuestionData>.Visit(
			GeneratedQuestionTemplate question,
			UserQuestionData userQuestionData)
		{
			return new GeneratedQuestionTemplateResolver
			(
				userQuestionData, 
				_jsonSerializer
			);
		}

		/// <summary>
		/// Creates a question loader for a randomly selected question.
		/// </summary>
		IQuestionResolver IQuestionResultVisitor<IQuestionResolver, UserQuestionData>.Visit(
			RandomlySelectedQuestion question,
			UserQuestionData userQuestionData)
		{
			return new RandomlySelectedQuestionResolver
			(
				userQuestionData, 
				_dbContext, 
				_questionLoaderFactory
			);
		}
	}
}
