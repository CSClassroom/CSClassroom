using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Exercises;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Data
{
	/// <summary>
	/// The database context.
	/// </summary>
    public class DatabaseContext : DbContext
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		public DatabaseContext(DbContextOptions<DatabaseContext> options)
			: base(options)
		{
		}

		/// <summary>
		/// The groups table.
		/// </summary>
		public DbSet<Group> Groups { get; set; }

		/// <summary>
		/// The classrooms table.
		/// </summary>
		public DbSet<Classroom> Classrooms { get; set; }

		/// <summary>
		/// The question categories table.
		/// </summary>
		public DbSet<QuestionCategory> QuestionCategories { get; set; }

		/// <summary>
		/// The questions table.
		/// </summary>
		public DbSet<Question> Questions { get; set; }

		/// <summary>
		/// Set of code questions in the questions table.
		/// Note: The name begins with an A, due to the following bug:
		/// https://github.com/aspnet/EntityFramework/issues/5547
		/// </summary>
		public DbSet<CodeQuestion> ACodeQuestions { get; set; }

		/// <summary>
		/// Set of class questions in the questions table.
		/// </summary>
		public DbSet<ClassQuestion> ClassQuestions { get; set; }

		/// <summary>
		/// Set of method questions in the questions table.
		/// </summary>
		public DbSet<MethodQuestion> MethodQuestions { get; set; }

		/// <summary>
		/// The code question tests table.
		/// </summary>
		public DbSet<CodeQuestionTest> CodeQuestionTests { get; set; }

		/// <summary>
		/// The class question tests in the code question tests table.
		/// </summary>
		public DbSet<ClassQuestionTest> ClassQuestionTests { get; set; }

		/// <summary>
		/// The method question tests in the code question tests table.
		/// </summary>
		public DbSet<MethodQuestionTest> MethodQuestionTests { get; set; }
	}
}
