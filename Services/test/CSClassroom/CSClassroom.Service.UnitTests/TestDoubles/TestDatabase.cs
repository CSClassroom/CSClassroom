using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.UnitTests.TestDoubles
{
	/// <summary>
	/// Creates a new database context.
	/// </summary>
	public class TestDatabase
	{
		/// <summary>
		/// The database store.
		/// </summary>
		private readonly TestDatabaseStore _store;

		/// <summary>
		/// The database context.
		/// </summary>
		public DatabaseContext Context { get; private set; }

		/// <summary>
		/// Creates an in-memory database.
		/// </summary>
		public TestDatabase(TestDatabaseStore store, DatabaseContext context)
		{
			_store = store;
			Context = context;
		}

		/// <summary>
		/// Simulates a reload of the database. This is useful to ensure
		/// that we only ever return entities that were saved to the store.
		/// </summary>
		public void Reload()
		{
			Context = new DatabaseContext(_store.Options);
		}
	}
}
