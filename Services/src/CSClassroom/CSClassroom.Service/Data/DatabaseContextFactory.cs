using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Data
{
	/// <summary>
	/// A database context factory. This is required for entity framework 
	/// migration tools to work. This class is not used for any other purpose.
	/// </summary>
	public class DatabaseContextFactory : IDbContextFactory<DatabaseContext>
	{
		/// <summary>
		/// Creates a database context.
		/// </summary>
		public DatabaseContext Create(DbContextFactoryOptions options)
		{
			var builder = new DbContextOptionsBuilder<DatabaseContext>();
			
			builder.UseNpgsql("User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=csclassroom;Pooling=true;");

			return new DatabaseContext(builder.Options);
		}
	}
}
