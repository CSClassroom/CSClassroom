using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A test class for a checkpoint.
	/// </summary>
	public class CheckpointTestClass
	{
		/// <summary>
		/// The unique ID for the checkpoint test class.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The checkpoint.
		/// </summary>
		public int CheckpointId { get; set; }

		/// <summary>
		/// The checkpoint.
		/// </summary>
		public Checkpoint Checkpoint { get; set; }

		/// <summary>
		/// The test class.
		/// </summary>
		[Display(Name = "Test Class")]
		public int TestClassId { get; set; }

		/// <summary>
		/// The test class.
		/// </summary>
		public TestClass TestClass { get; set; }

		/// <summary>
		/// Whether or not tests in the class are required for the checkpoints.
		/// </summary>
		[Display(Name = "Required")]
		public bool Required { get; set; }
	}
}
