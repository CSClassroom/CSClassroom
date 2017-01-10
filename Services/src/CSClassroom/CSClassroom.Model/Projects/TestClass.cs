using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A test class that will run when project tests are run.
	/// </summary>
	public class TestClass
	{
		/// <summary>
		/// The primary key.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the project that this test class belongs to.
		/// </summary>
		public int ProjectId { get; set; }

		/// <summary>
		/// The project project that this test class belongs to.
		/// </summary>
		public Project Project { get; set; }

		/// <summary>
		/// Checkpoints that use this test class.
		/// </summary>
		public IList<CheckpointTestClass> CheckpointTestClasses { get; set; }

		/// <summary>
		/// The name of the test class.
		/// </summary>
		[Display(Name = "Class Name")]
		public string ClassName { get; set; }

		/// <summary>
		/// The display name for the class.
		/// </summary>
		[Display(Name = "Display Name")]
		public string DisplayName { get; set; }

		/// <summary>
		/// The order test classes are displayed in.
		/// </summary>
		public int Order { get; set; }
	}
}
