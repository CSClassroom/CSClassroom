using System;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Checkpoints
{
	/// <summary>
	/// UnitTests for the CheckpointService class.
	/// </summary>
	public class CheckpointService_UnitTests
	{
		/// <summary>
		/// Ensures that GetCheckpointsAsync returns only checkpoints
		/// for a given classroom.
		/// </summary>
		[Fact]
		public async Task GetCheckpointsAsync_OnlyForProject()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddProject("Class1", "Project1")
				.AddProject("Class1", "Project2")
				.AddCheckpoint("Class1", "Project1", "Checkpoint1")
				.AddCheckpoint("Class1", "Project1", "Checkpoint2")
				.AddCheckpoint("Class1", "Project2", "Checkpoint1")
				.Build();

			var checkpointService = GetCheckpointService(database.Context);
			var checkpoints = await checkpointService.GetCheckpointsAsync("Class1", "Project1");

			Assert.Equal(2, checkpoints.Count);
			Assert.Equal("Project1", checkpoints[0].Project.Name);
			Assert.Equal("Checkpoint1", checkpoints[0].Name);
			Assert.Equal("Project1", checkpoints[1].Project.Name);
			Assert.Equal("Checkpoint2", checkpoints[1].Name);
		}

		/// <summary>
		/// Ensures that GetCheckpointAsync returns the desired
		/// checkpoint, if it exists.
		/// </summary>
		[Fact]
		public async Task GetCheckpointAsync_Exists_ReturnCheckpoint()
		{
			var database = GetDatabaseBuilderWithCheckpoint().Build();

			var checkpointService = GetCheckpointService(database.Context);
			var checkpoint = await checkpointService.GetCheckpointAsync
			(
				"Class1", 
				"Project1", 
				"Checkpoint1"
			);

			Assert.Equal("Class1", checkpoint.Project.Classroom.Name);
			Assert.Equal("Project1", checkpoint.Project.Name);
			Assert.Equal("Checkpoint1", checkpoint.Name);
			Assert.Equal(2, checkpoint.SectionDates.Count);
			Assert.Equal(2, checkpoint.TestClasses.Count);
		}

		/// <summary>
		/// Ensures that GetCheckpointAsync returns null, if the desired
		/// checkpoint doesn't exist.
		/// </summary>
		[Fact]
		public async Task GetCheckpointAsync_DoesntExist_ReturnNull()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddProject("Class1", "Project1")
				.Build();

			var checkpointService = GetCheckpointService(database.Context);
			var checkpoint = await checkpointService.GetCheckpointAsync
			(
				"Class1", 
				"Project1", 
				"Checkpoint1"
			);

			Assert.Null(checkpoint);
		}

		/// <summary>
		/// Ensures that CreateCheckpointAsync actually creates the checkpoint.
		/// </summary>
		[Fact]
		public async Task CreateCheckpointAsync_CheckpointCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddSection("Class1", "Period2")
				.AddProject("Class1", "Project1")
				.AddProjectTestClass("Class1", "Project1", "TestClass1")
				.AddProjectTestClass("Class1", "Project1", "TestClass2")
				.Build();

			var testClasses = database.Context.TestClasses.ToList();
			var sections = database.Context.Sections.ToList();
			database.Reload();

			var modelErrors = new MockErrorCollection();
			var checkpointService = GetCheckpointService(database.Context);

			var result = await checkpointService.CreateCheckpointAsync
			(
				"Class1",
				"Project1",
				new Checkpoint()
				{
					Name = "Checkpoint1",
					DisplayName = "Checkpoint1 DisplayName",
					TestClasses = Collections.CreateList
					(
						new CheckpointTestClass() { TestClassId = testClasses[0].Id },
						new CheckpointTestClass() { TestClassId = testClasses[1].Id }
					),
					SectionDates = Collections.CreateList
					(
						new CheckpointDates()
						{
							SectionId = sections[0].Id,
							DueDate = Period1DueDate
						},
						new CheckpointDates()
						{
							SectionId = sections[1].Id,
							DueDate = Period2DueDate
						}
					)
				},
				modelErrors
			);

			database.Reload();

			var checkpoint = database.Context.Checkpoints
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.TestClasses)
				.Include(c => c.SectionDates)
				.Single();

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);

			Assert.Equal("Class1", checkpoint.Project.Classroom.Name);
			Assert.Equal("Project1", checkpoint.Project.Name);
			Assert.Equal("Checkpoint1", checkpoint.Name);

			Assert.Equal(2, checkpoint.TestClasses.Count);
			Assert.Equal(testClasses[0].Id, checkpoint.TestClasses[0].TestClassId);
			Assert.Equal(testClasses[1].Id, checkpoint.TestClasses[1].TestClassId);

			Assert.Equal(2, checkpoint.SectionDates.Count);
			Assert.Equal(sections[0].Id, checkpoint.SectionDates[0].SectionId);
			Assert.Equal(Period1DueDate, checkpoint.SectionDates[0].DueDate);
			Assert.Equal(sections[1].Id, checkpoint.SectionDates[1].SectionId);
			Assert.Equal(Period2DueDate, checkpoint.SectionDates[1].DueDate);
		}

		/// <summary>
		/// Ensures that CreateCheckpointAsync does not create the checkpoint
		/// if there are duplicate test classes.
		/// </summary>
		[Fact]
		public async Task CreateCheckpointAsync_DuplicateTestClasses_CheckpointNotCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddProject("Class1", "Project1")
				.AddProjectTestClass("Class1", "Project1", "TestClass1")
				.AddProjectTestClass("Class1", "Project1", "TestClass2")
				.Build();

			var testClasses = database.Context.TestClasses.ToList();
			var sections = database.Context.Sections.ToList();
			database.Reload();

			var modelErrors = new MockErrorCollection();
			var checkpointService = GetCheckpointService(database.Context);

			var result = await checkpointService.CreateCheckpointAsync
			(
				"Class1",
				"Project1",
				new Checkpoint()
				{
					Name = "Checkpoint1",
					DisplayName = "Checkpoint1 DisplayName",
					TestClasses = Collections.CreateList
					(
						new CheckpointTestClass() { TestClassId = testClasses[0].Id },
						new CheckpointTestClass() { TestClassId = testClasses[0].Id }
					),
				},
				modelErrors
			);

			database.Reload();
			var checkpoint = database.Context.Checkpoints.SingleOrDefault();

			Assert.False(result);
			Assert.True(modelErrors.HasErrors);
			Assert.True(modelErrors.VerifyErrors("TestClasses"));
			Assert.Null(checkpoint);
		}

		/// <summary>
		/// Ensures that CreateCheckpointAsync does not create the checkpoint
		/// if there are multiple due dates for the same section.
		/// </summary>
		[Fact]
		public async Task CreateCheckpointAsync_DuplicateSectionDueDates_CheckpointNotCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddSection("Class1", "Period2")
				.AddProject("Class1", "Project1")
				.Build();
			
			var sections = database.Context.Sections.ToList();
			database.Reload();

			var modelErrors = new MockErrorCollection();
			var checkpointService = GetCheckpointService(database.Context);

			var result = await checkpointService.CreateCheckpointAsync
			(
				"Class1",
				"Project1",
				new Checkpoint()
				{
					Name = "Checkpoint1",
					DisplayName = "Checkpoint1 DisplayName",
					SectionDates = Collections.CreateList
					(
						new CheckpointDates()
						{
							SectionId = sections[0].Id,
							DueDate = Period1DueDate
						},
						new CheckpointDates()
						{
							SectionId = sections[0].Id,
							DueDate = Period2DueDate
						}
					)
				},
				modelErrors
			);

			database.Reload();
			var checkpoint = database.Context.Checkpoints.SingleOrDefault();

			Assert.False(result);
			Assert.True(modelErrors.HasErrors);
			Assert.True(modelErrors.VerifyErrors("SectionDates"));
			Assert.Null(checkpoint);
		}

		/// <summary>
		/// Ensures that UpdateCheckpointAsync actually updates the checkpoint.
		/// </summary>
		[Fact]
		public async Task UpdateCheckpointAsync_ValidModification_CheckpointUpdated()
		{
			var database = GetDatabaseBuilderWithCheckpoint().Build();

			var checkpoint = database.Context.Checkpoints
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.TestClasses)
				.Include(c => c.SectionDates)
				.Single();

			var sections = database.Context.Sections.ToList();
			var testClasses = database.Context.TestClasses.ToList();
			
			database.Reload();

			// Update the checkpoint

			checkpoint.TestClasses.RemoveAt(0);
			checkpoint.TestClasses.Add
			(
				new CheckpointTestClass()
				{
					TestClassId = testClasses[0].Id,
					Required = false
				}
			);

			checkpoint.SectionDates.RemoveAt(0);
			checkpoint.SectionDates.Add
			(
				new CheckpointDates()
				{
					SectionId = sections[0].Id,
					DueDate = DateTime.MinValue
				}
			);

			// Apply the update
			var modelErrors = new MockErrorCollection();
			var checkpointService = GetCheckpointService(database.Context);
			var result = await checkpointService.UpdateCheckpointAsync
			(
				"Class1", 
				"Project1", 
				checkpoint, 
				modelErrors
			);

			database.Reload();

			checkpoint = database.Context.Checkpoints
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.TestClasses)
				.Include(c => c.SectionDates)
				.Single();

			var checkpointTestClasses = checkpoint.TestClasses
				.OrderBy(tc => tc.Id)
				.ToList();

			var checkpointDates = checkpoint.SectionDates
				.OrderBy(cd => cd.Id)
				.ToList();

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);

			Assert.Equal("Class1", checkpoint.Project.Classroom.Name);
			Assert.Equal("Project1", checkpoint.Project.Name);
			Assert.Equal("Checkpoint1", checkpoint.Name);

			Assert.Equal(2, checkpoint.TestClasses.Count);
			Assert.Equal(testClasses[1].Id, checkpointTestClasses[0].TestClassId);
			Assert.True(checkpointTestClasses[0].Required);
			Assert.Equal(testClasses[0].Id, checkpointTestClasses[1].TestClassId);
			Assert.False(checkpointTestClasses[1].Required);

			Assert.Equal(2, checkpoint.SectionDates.Count);
			Assert.Equal(sections[1].Id, checkpointDates[0].SectionId);
			Assert.Equal(Period2DueDate, checkpointDates[0].DueDate);
			Assert.Equal(sections[0].Id, checkpointDates[1].SectionId);
			Assert.Equal(DateTime.MinValue, checkpointDates[1].DueDate);
		}

		/// <summary>
		/// Ensures that UpdateCheckpointAsync does not update the checkpoint
		/// if there is a record for more than one test class.
		/// </summary>
		[Fact]
		public async Task UpdateCheckpointAsync_DuplicateTestClasses_CheckpointNotUpdated()
		{
			var database = GetDatabaseBuilderWithCheckpoint().Build();

			var checkpoint = database.Context.Checkpoints
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.TestClasses)
				.Include(c => c.SectionDates)
				.Single();
			
			var testClasses = database.Context.TestClasses.ToList();

			database.Reload();

			// Update the checkpoint
			checkpoint.TestClasses.Add
			(
				new CheckpointTestClass()
				{
					TestClassId = testClasses[0].Id,
					Required = false
				}
			);

			// Apply the update
			var modelErrors = new MockErrorCollection();
			var checkpointService = GetCheckpointService(database.Context);
			var result = await checkpointService.UpdateCheckpointAsync
			(
				"Class1",
				"Project1",
				checkpoint,
				modelErrors
			);

			database.Reload();

			checkpoint = database.Context.Checkpoints
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.TestClasses)
				.Include(c => c.SectionDates)
				.Single();

			Assert.False(result);
			Assert.True(modelErrors.HasErrors);
			Assert.True(modelErrors.VerifyErrors("TestClasses"));

			Assert.Equal(2, checkpoint.TestClasses.Count);
			Assert.Equal(testClasses[0].Id, checkpoint.TestClasses[0].TestClassId);
			Assert.True(checkpoint.TestClasses[0].Required);
			Assert.Equal(testClasses[1].Id, checkpoint.TestClasses[1].TestClassId);
			Assert.True(checkpoint.TestClasses[1].Required);
		}

		/// <summary>
		/// Ensures that UpdateCheckpointAsync does not update the checkpoint
		/// if there are multiple due dates for the same section.
		/// </summary>
		[Fact]
		public async Task UpdateCheckpointAsync_DuplicateSectionDueDates_CheckpointNotUpdated()
		{
			var database = GetDatabaseBuilderWithCheckpoint().Build();

			var checkpoint = database.Context.Checkpoints
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.TestClasses)
				.Include(c => c.SectionDates)
				.Single();

			var sections = database.Context.Sections.ToList();

			database.Reload();

			// Update the checkpoint
			checkpoint.SectionDates.Add
			(
				new CheckpointDates()
				{
					SectionId = sections[0].Id,
					DueDate = DateTime.MinValue
				}
			);

			// Apply the update
			var modelErrors = new MockErrorCollection();
			var checkpointService = GetCheckpointService(database.Context);
			var result = await checkpointService.UpdateCheckpointAsync
			(
				"Class1",
				"Project1",
				checkpoint,
				modelErrors
			);

			database.Reload();

			checkpoint = database.Context.Checkpoints
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.TestClasses)
				.Include(c => c.SectionDates)
				.Single();

			Assert.False(result);
			Assert.True(modelErrors.HasErrors);
			Assert.True(modelErrors.VerifyErrors("SectionDates"));

			Assert.Equal(2, checkpoint.TestClasses.Count);
			Assert.Equal(sections[0].Id, checkpoint.SectionDates[0].SectionId);
			Assert.Equal(Period1DueDate, checkpoint.SectionDates[0].DueDate);
			Assert.Equal(sections[1].Id, checkpoint.SectionDates[1].SectionId);
			Assert.Equal(Period2DueDate, checkpoint.SectionDates[1].DueDate);
		}

		/// <summary>
		/// Ensures that DeleteCheckpointAsync actually deletes a checkpoint.
		/// </summary>
		[Fact]
		public async Task DeleteCheckpointAsync_CheckpointDeleted()
		{
			var database = GetDatabaseBuilderWithCheckpoint().Build();

			var checkpointService = GetCheckpointService(database.Context);
			await checkpointService.DeleteCheckpointAsync
			(
				"Class1", 
				"Project1", 
				"Checkpoint1"
			);

			database.Reload();

			Assert.Equal(0, database.Context.Checkpoints.Count());
		}

		/// <summary>
		/// Returns a database with a project.
		/// </summary>
		private TestDatabaseBuilder GetDatabaseBuilderWithCheckpoint()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddSection("Class1", "Period2")
				.AddProject("Class1", "Project1")
				.AddProjectTestClass("Class1", "Project1", "TestClass1")
				.AddProjectTestClass("Class1", "Project1", "TestClass2")
				.AddCheckpoint("Class1", "Project1", "Checkpoint1")
				.AddCheckpointTestClass("Class1", "Project1", "Checkpoint1", "TestClass1", required: true)
				.AddCheckpointTestClass("Class1", "Project1", "Checkpoint1", "TestClass2", required: true)
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period1", Period1DueDate)
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period2", Period2DueDate);
		}

		/// <summary>
		/// The checkpoint due date for period 1.
		/// </summary>
		private static readonly DateTime Period1DueDate = new DateTime(2016, 1, 1);

		/// <summary>
		/// The checkpoint due date for period 2.
		/// </summary>
		private static readonly DateTime Period2DueDate = new DateTime(2016, 1, 2);

		private ICheckpointService GetCheckpointService(DatabaseContext dbContext)
		{
			return new CheckpointService(dbContext);
		}
	}
}
