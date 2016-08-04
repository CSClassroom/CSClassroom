using Autofac;
using CSC.CodeRunner.Service;

namespace CSC.CodeRunner.Service.Configuration
{
	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
    public static class ContainerBuilderExtensions
    {
		/// <summary>
		/// Registers dependencies for the code runner service.
		/// </summary>
		/// <param name="builder">The IOC container builder.</param>
		public static void RegisterCodeRunnerService(this ContainerBuilder builder)
		{
			builder.RegisterType<CodeRunnerService>().As<ICodeRunnerService>();
		}
    }
}
