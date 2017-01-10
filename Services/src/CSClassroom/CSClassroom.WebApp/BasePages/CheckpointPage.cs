using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.WebApp.BasePages
{
	/// <summary>
	/// The base page for a view using in a particular checkpoint.
	/// </summary>
	public abstract class CheckpointPage<TModel> : ProjectPage<TModel>
	{
		/// <summary>
		/// The current checkpoint.
		/// </summary>
		public Checkpoint Checkpoint => ViewBag.Checkpoint;
	}
}
