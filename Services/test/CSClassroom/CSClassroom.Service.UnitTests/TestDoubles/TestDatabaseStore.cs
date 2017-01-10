using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSC.CSClassroom.Service.UnitTests.TestDoubles
{
	/// <summary>
	/// A store for a test database.
	/// </summary>
	public class TestDatabaseStore
	{
		/// <summary>
		/// Options for a database context.
		/// </summary>
		public DbContextOptions<DatabaseContext> Options { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TestDatabaseStore()
		{
			var serviceProvider = new ServiceCollection()
				.AddEntityFrameworkInMemoryDatabase()
				.BuildServiceProvider();

			var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
			optionsBuilder.UseInMemoryDatabase()
				.UseInternalServiceProvider(serviceProvider);

			Options = optionsBuilder.Options;
		}
	}
}
