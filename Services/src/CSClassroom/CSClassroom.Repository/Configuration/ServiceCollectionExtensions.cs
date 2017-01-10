using System;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CSC.CSClassroom.Repository.Configuration
{
	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers the CSClassroom database.
		/// </summary>
		public static void AddCSClassroomDatabase(
			this IServiceCollection services,
			string connectionString)
		{
			// Add the database.
			services.AddDbContext<DatabaseContext>
			(
				options => options.UseNpgsql(connectionString)
			);
		}
	}
}
