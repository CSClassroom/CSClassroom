using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs checkpoint operations.
	/// </summary>
	public interface ICheckpointService
	{
		/// <summary>
		/// Returns the list of checkpoints.
		/// </summary>
		Task<IList<Checkpoint>> GetCheckpointsAsync(
			string classroomName, 
			string projectName);

		/// <summary>
		/// Returns the checkpoint with the given name.
		/// </summary>
		Task<Checkpoint> GetCheckpointAsync(
			string classroomName,
			string projectName,
			string checkpointName);

		/// <summary>
		/// Creates a checkpoint.
		/// </summary>
		Task<bool> CreateCheckpointAsync(
			string classroomName,
			string projectName,
			Checkpoint checkpoint, 
			IModelErrorCollection modelErrors);

		/// <summary>
		/// Updates a checkpoint.
		/// </summary>
		Task<bool> UpdateCheckpointAsync(
			string classroomName,
			string projectName,
			Checkpoint checkpoint,
			IModelErrorCollection modelErrors);

		/// <summary>
		/// Removes a checkpoint.
		/// </summary>
		Task DeleteCheckpointAsync(
			string classroomName,
			string projectName,
			string checkpointName);
	}
}
