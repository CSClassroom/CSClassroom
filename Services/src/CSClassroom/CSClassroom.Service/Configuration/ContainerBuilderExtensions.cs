using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Questions;
using CSC.CSClassroom.Service.Questions.AssignmentScoring;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.Service.Projects.PushEvents;
using CSC.CSClassroom.Service.Projects.Repositories;
using CSC.CSClassroom.Service.Projects.Submissions;
using Microsoft.Extensions.Configuration;

namespace CSC.CSClassroom.Service.Configuration
{
	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
	public static class ContainerBuilderExtensions
	{
		/// <summary>
		/// Registers dependencies for CSClassroom services.
		/// </summary>
		public static void RegisterCSClassroomService(
			this ContainerBuilder builder, 
			IConfigurationSection serviceSettings)
		{
			builder.RegisterSource
			(
				new AnyConcreteTypeNotAlreadyRegisteredSource
				(
					type => type.GetTypeInfo().Assembly.Equals
					(
						typeof(ContainerBuilderExtensions).GetTypeInfo().Assembly
					)
				)
			);

			builder.RegisterInstance(GetActivationToken(serviceSettings)).As<ActivationToken>();
			builder.RegisterType<UserProvider>().As<IUserProvider>();

			builder.RegisterType<UserService>().As<IUserService>();
			builder.RegisterType<ClassroomService>().As<IClassroomService>();
			builder.RegisterType<SectionService>().As<ISectionService>();
			builder.RegisterType<QuestionCategoryService>().As<IQuestionCategoryService>();
			builder.RegisterType<QuestionService>().As<IQuestionService>();
			builder.RegisterType<AssignmentService>().As<IAssignmentService>();
			builder.RegisterType<ProjectService>().As<IProjectService>();
			builder.RegisterType<CheckpointService>().As<ICheckpointService>();
			builder.RegisterType<SubmissionService>().As<ISubmissionService>();
			builder.RegisterType<Projects.BuildService>().As<IBuildService>();
			builder.RegisterType<QuestionGenerator>().As<IQuestionGenerator>();
			builder.RegisterType<JavaCodeGenerationFactory>().As<IJavaCodeGenerationFactory>();
			builder.RegisterType<QuestionModelFactory>().As<IQuestionModelFactory>();
			builder.RegisterType<AssignmentScoreCalculator>().As<IAssignmentScoreCalculator>();
			builder.RegisterType<PushEventRetriever>().As<IPushEventRetriever>();
			builder.RegisterType<RepositoryMetadataRetriever>().As<IRepositoryMetadataRetriever>();
			builder.RegisterType<RepositoryPopulator>().As<IRepositoryPopulator>();
			builder.RegisterType<PushEventProcessor>().As<IPushEventProcessor>();
			builder.RegisterType<SubmissionCreator>().As<ISubmissionCreator>();
			builder.RegisterType<SubmissionDownloader>().As<ISubmissionDownloader>();
			builder.RegisterType<SubmissionArchiveBuilder>().As<ISubmissionArchiveBuilder>();
			builder.RegisterType<SubmissionFileTransformer>().As<ISubmissionFileTransformer>();
		}

		/// <summary>
		/// Registers web app settings.
		/// </summary>
		private static ActivationToken GetActivationToken(IConfigurationSection serviceSettings)
		{
			return new ActivationToken(serviceSettings["ActivationToken"]);
		}
	}
}
