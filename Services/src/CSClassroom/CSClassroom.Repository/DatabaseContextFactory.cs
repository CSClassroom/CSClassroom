using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace CSC.CSClassroom.Repository
{
	/// <summary>
	/// A database context factory. This class is used by the entity framework
	/// migration tools ("dotnet ef ...") when initially populating the database, 
	/// and when updating the database to apply a migration. 
	/// </summary>
	public class DatabaseContextFactory : IDbContextFactory<DatabaseContext>
	{
		/// <summary>
		/// The file containing the database's connection strings.
		/// </summary>
		private const string c_settingsFileName = "DatabaseConnectionStrings.json";

		/// <summary>
		/// Creates a database context.
		/// </summary>
		public DatabaseContext Create(DbContextFactoryOptions options)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(options.ContentRootPath)
				.AddJsonFile(c_settingsFileName)
				.Build();

			var connectionString = configuration[options.EnvironmentName];

			if (connectionString == null)
			{
				throw new InvalidOperationException(
					  $"No connection string specified in {c_settingsFileName} "
					+ $"for environment '{options.EnvironmentName}'.");
			}

			var builder = new DbContextOptionsBuilder<DatabaseContext>();

			builder.UseNpgsql(connectionString);

			return new DatabaseContext(builder.Options);
		}
	}
}
