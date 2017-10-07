using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace CSC.CSClassroom.Repository
{
	/// <summary>
	/// A database context factory. This class is used by the entity framework
	/// migration tools ("dotnet ef ...") when initially populating the database, 
	/// and when updating the database to apply a migration. 
	/// </summary>
	public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
	{
		/// <summary>
		/// The file containing the database's connection strings.
		/// </summary>
		private const string c_settingsFileName = "DatabaseConnectionStrings.json";

		/// <summary>
		/// Creates a database context.
		/// </summary>
		public DatabaseContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(c_settingsFileName)
				.Build();

			var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var connectionString = configuration[environmentName];

			if (connectionString == null)
			{
				throw new InvalidOperationException(
					  $"No connection string specified in {c_settingsFileName} "
					+ $"for environment '{environmentName}'.");
			}

			var builder = new DbContextOptionsBuilder<DatabaseContext>();

			builder.UseNpgsql(connectionString);

			return new DatabaseContext(builder.Options);
		}
	}
}
