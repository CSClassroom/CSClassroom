using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs checkpoint operations.
	/// </summary>
	public class CheckpointService : ICheckpointService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public CheckpointService(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns the list of checkpoints.
		/// </summary>
		public async Task<IList<Checkpoint>> GetCheckpointsAsync(
			string classroomName, 
			string projectName)
		{
			var project = await LoadProjectAsync(classroomName, projectName);

			return await _dbContext.Checkpoints
				.Where(checkpoint => checkpoint.ProjectId == project.Id)
				.Include(checkpoint => checkpoint.SectionDates)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the checkpoint with the given name.
		/// </summary>
		public async Task<Checkpoint> GetCheckpointAsync(
			string classroomName,
			string projectName,
			string checkpointName)
		{
			var project = await LoadProjectAsync(classroomName, projectName);

			return await _dbContext.Checkpoints
				.Where(checkpoint => checkpoint.ProjectId == project.Id)
				.Include(checkpoint => checkpoint.Project)
				.Include(checkpoint => checkpoint.Project.Classroom)
				.Include(checkpoint => checkpoint.TestClasses)
				.Include(checkpoint => checkpoint.SectionDates)
				.SingleOrDefaultAsync(checkpoint => checkpoint.Name == checkpointName);
		}

		/// <summary>
		/// Creates a checkpoint.
		/// </summary>
		public async Task<bool> CreateCheckpointAsync(
			string classroomName,
			string projectName,
			Checkpoint checkpoint, 
			IModelErrorCollection modelErrors)
		{
			var project = await LoadProjectAsync(classroomName, projectName);

			if (!UpdateCheckpoint(checkpoint, modelErrors))
				return false;

			checkpoint.ProjectId = project.Id;
			_dbContext.Add(checkpoint);

			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Updates a checkpoint.
		/// </summary>
		public async Task<bool> UpdateCheckpointAsync(
			string classroomName,
			string projectName,
			Checkpoint checkpoint,
			IModelErrorCollection modelErrors)
		{
			var project = await LoadProjectAsync(classroomName, projectName);

			checkpoint.ProjectId = project.Id;

			var currentCheckpoint = await _dbContext.Checkpoints
				.Where(c => c.Id == checkpoint.Id)
				.SingleOrDefaultAsync();

			_dbContext.Entry(currentCheckpoint).State = EntityState.Detached;

			if (!UpdateCheckpoint(checkpoint, modelErrors))
				return false;

			_dbContext.Update(checkpoint);

			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Removes a checkpoint.
		/// </summary>
		public async Task DeleteCheckpointAsync(
			string classroomName,
			string projectName, 
			string checkpointName)
		{
			var checkpoint = await GetCheckpointAsync
			(
				classroomName, 
				projectName, 
				checkpointName
			);

			_dbContext.Checkpoints.Remove(checkpoint);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a checkpoint.
		/// </summary>
		private bool UpdateCheckpoint(Checkpoint checkpoint, IModelErrorCollection modelErrors)
		{
			if (checkpoint.SectionDates != null)
			{
				var sections = checkpoint.SectionDates.Select(d => d.SectionId).ToList();
				if (sections.Distinct().Count() != sections.Count)
				{
					modelErrors.AddError("SectionDates", "You may only have one due date per section.");
				}
			}

			if (checkpoint.TestClasses != null)
			{
				var testClasses = checkpoint.TestClasses.Select(tc => tc.TestClassId).ToList();
				if (testClasses.Distinct().Count() != testClasses.Count)
				{
					modelErrors.AddError("TestClasses", "You may only have one entry per test class.");
				}
			}

			if (modelErrors.HasErrors)
			{
				return false;
			}

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.CheckpointDates,
				checkpointDates => checkpointDates.Id,
				checkpointDates => checkpointDates.CheckpointId == checkpoint.Id,
				checkpoint.SectionDates
			);

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.CheckpointTestClasses,
				testClass => testClass.Id,
				testClass => testClass.CheckpointId == checkpoint.Id,
				checkpoint.TestClasses
			);

			return true;
		}

		/// <summary>
		/// Loads a project from the database.
		/// </summary>
		private async Task<Project> LoadProjectAsync(string classroomName, string projectName)
		{
			return await _dbContext.Projects
				.Where(p => p.Classroom.Name == classroomName)
				.SingleAsync(p => p.Name == projectName);
		}
	}
}
