using Autofac;
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
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using CSC.CSClassroom.Service.Questions.QuestionGraders;
using CSC.CSClassroom.Service.Questions.QuestionLoaders;
using CSC.CSClassroom.Service.Questions.QuestionResolvers;
using CSC.CSClassroom.Service.Questions.QuestionSolvers;
using CSC.CSClassroom.Service.Questions.QuestionUpdaters;
using CSC.CSClassroom.Service.Questions.UserQuestionDataLoaders;
using CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters;
using CSC.CSClassroom.Service.Questions.Validators;
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
			RegisterIdentityComponents(builder, serviceSettings);
			RegisterClassroomComponents(builder);
			RegisterAssignmentComponents(builder);
			RegisterProjectComponents(builder);
		}

		/// <summary>
		/// Registers the user service. 
		/// </summary>
		private static void RegisterIdentityComponents(
			this ContainerBuilder builder,
			IConfigurationSection serviceSettings)
		{
			builder.RegisterType<UserService>().As<IUserService>();
			builder.RegisterType<UserProvider>().As<IUserProvider>();
			builder.RegisterInstance(GetActivationToken(serviceSettings)).As<ActivationToken>();
		}

		/// <summary>
		/// Returns the activation token for the service.
		/// </summary>
		private static ActivationToken GetActivationToken(IConfigurationSection serviceSettings)
		{
			return new ActivationToken(serviceSettings["ActivationToken"]);
		}

		/// <summary>
		/// Registers services related to classrooms. 
		/// </summary>
		private static void RegisterClassroomComponents(
			this ContainerBuilder builder)
		{
			builder.RegisterType<ClassroomService>().As<IClassroomService>();
			builder.RegisterType<SectionService>().As<ISectionService>();
		}

		/// <summary>
		/// Registers dependencies related to assignments.
		/// </summary>
		private static void RegisterAssignmentComponents(ContainerBuilder builder)
		{
			RegisterAssignmentServices(builder);
			RegisterAssignmentScoringComponents(builder);
			RegisterAssignmentValidationComponents(builder);
			RegisterQuestionComponents(builder);
		}

		/// <summary>
		/// Registers services related to assignments.
		/// </summary>
		private static void RegisterAssignmentServices(ContainerBuilder builder)
		{
			builder.RegisterType<AssignmentQuestionService>().As<IAssignmentQuestionService>();
			builder.RegisterType<AssignmentService>().As<IAssignmentService>();
			builder.RegisterType<QuestionCategoryService>().As<IQuestionCategoryService>();
			builder.RegisterType<QuestionService>().As<IQuestionService>();
		}

		/// <summary>
		/// Registers components related to assignment scoring.
		/// </summary>
		private static void RegisterAssignmentScoringComponents(ContainerBuilder builder)
		{
			builder.RegisterType<AssignmentFilter>().As<IAssignmentFilter>();
			builder.RegisterType<AssignmentGroupScoreCalculator>().As<IAssignmentGroupScoreCalculator>();
			builder.RegisterType<AssignmentGroupResultGenerator>().As<IAssignmentGroupResultGenerator>();
			builder.RegisterType<AssignmentResultGenerator>().As<IAssignmentResultGenerator>();
			builder.RegisterType<AssignmentScoreCalculator>().As<IAssignmentScoreCalculator>();
			builder.RegisterType<QuestionResultGenerator>().As<IQuestionResultGenerator>();
			builder.RegisterType<QuestionScoreCalculator>().As<IQuestionScoreCalculator>();
			builder.RegisterType<SectionAssignmentReportGenerator>().As<ISectionAssignmentReportGenerator>();
			builder.RegisterType<SnapshotAssignmentReportGenerator>().As<ISnapshotAssignmentReportGenerator>();
			builder.RegisterType<StudentAssignmentReportGenerator>().As<IStudentAssignmentReportGenerator>();
			builder.RegisterType<SubmissionStatusCalculator>().As<ISubmissionStatusCalculator>();
			builder.RegisterType<UpdatedAssignmentReportGenerator>().As<IUpdatedAssignmentReportGenerator>();
		}

		/// <summary>
		/// Registers components related to validating assignments and related types.
		/// </summary>
		private static void RegisterAssignmentValidationComponents(ContainerBuilder builder)
		{
			builder.RegisterType<AssignmentValidator>().As<IAssignmentValidator>();
			builder.RegisterType<QuestionValidator>().As<IQuestionValidator>();
		}

		/// <summary>
		/// Registers components related to questions.
		/// </summary>
		private static void RegisterQuestionComponents(ContainerBuilder builder)
		{
			builder.RegisterType<AssignmentDueDateRetriever>().As<IAssignmentDueDateRetriever>();
			builder.RegisterType<GeneratedQuestionSeedGenerator>().As<IGeneratedQuestionSeedGenerator>();
			builder.RegisterType<JavaCodeGenerationFactory>().As<IJavaCodeGenerationFactory>();
			builder.RegisterType<QuestionDuplicatorFactory>().As<IQuestionDuplicatorFactory>();
			builder.RegisterType<QuestionGenerator>().As<IQuestionGenerator>();
			builder.RegisterType<QuestionGraderFactory>().As<IQuestionGraderFactory>();
			builder.RegisterType<QuestionLoaderFactory>().As<IQuestionLoaderFactory>();
			builder.RegisterType<QuestionModelFactory>().As<IQuestionModelFactory>();
			builder.RegisterType<QuestionResolverFactory>().As<IQuestionResolverFactory>();
			builder.RegisterType<QuestionSolver>().As<IQuestionSolver>();
			builder.RegisterType<QuestionUpdaterFactory>().As<IQuestionUpdaterFactory>();
			builder.RegisterType<UnsolvedPrereqsRetriever>().As<IUnsolvedPrereqsRetriever>();
			builder.RegisterType<RandomlySelectedQuestionSelector>().As<IRandomlySelectedQuestionSelector>();
			builder.RegisterType<UserQuestionDataLoaderFactory>().As<IUserQuestionDataLoaderFactory>();
			builder.RegisterType<UserQuestionDataUpdaterFactory>().As<IUserQuestionDataUpdaterFactory>();
			builder.RegisterType<UserQuestionDataUpdaterImplFactory>().As<IUserQuestionDataUpdaterImplFactory>();
		}

		/// <summary>
		/// Registers components related to projects.
		/// </summary>
		private static void RegisterProjectComponents(ContainerBuilder builder)
		{
			RegisterProjectServices(builder);
			RegisterPushEventComponents(builder);
			RegisterRepositoryComponents(builder);
			RegisterSubmissionComponents(builder);
		}

		/// <summary>
		/// Registers services related to projects.
		/// </summary>
		private static void RegisterProjectServices(ContainerBuilder builder)
		{
			builder.RegisterType<ProjectService>().As<IProjectService>();
			builder.RegisterType<CheckpointService>().As<ICheckpointService>();
			builder.RegisterType<SubmissionService>().As<ISubmissionService>();
			builder.RegisterType<Projects.BuildService>().As<IBuildService>();
		}

		/// <summary>
		/// Registers components related to push events.
		/// </summary>
		/// <param name="builder"></param>
		private static void RegisterPushEventComponents(ContainerBuilder builder)
		{
			builder.RegisterType<PushEventRetriever>().As<IPushEventRetriever>();
			builder.RegisterType<PushEventProcessor>().As<IPushEventProcessor>();
		}

		/// <summary>
		/// Registers components related to repositories.
		/// </summary>
		private static void RegisterRepositoryComponents(ContainerBuilder builder)
		{
			builder.RegisterType<RepositoryMetadataRetriever>().As<IRepositoryMetadataRetriever>();
			builder.RegisterType<RepositoryPopulator>().As<IRepositoryPopulator>();
		}

		/// <summary>
		/// Registers components related to project submissions.
		/// </summary>
		private static void RegisterSubmissionComponents(ContainerBuilder builder)
		{
			builder.RegisterType<SubmissionCreator>().As<ISubmissionCreator>();
			builder.RegisterType<SubmissionDownloader>().As<ISubmissionDownloader>();
			builder.RegisterType<SubmissionArchiveBuilder>().As<ISubmissionArchiveBuilder>();
			builder.RegisterType<SubmissionFileTransformer>().As<ISubmissionFileTransformer>();
		}
	}
}
