using Autofac;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Exercises;

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
		/// <param name="builder">The IOC container builder.</param>
		public static void RegisterCSClassroomService(this ContainerBuilder builder)
		{
			builder.RegisterType<GroupService>().As<IGroupService>();
			builder.RegisterType<ClassroomService>().As<IClassroomService>();
			builder.RegisterType<QuestionCategoryService>().As<IQuestionCategoryService>();
			builder.RegisterType<QuestionService>().As<IQuestionService>();
		}
    }
}
