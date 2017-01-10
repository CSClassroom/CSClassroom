using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Classrooms
{
	/// <summary>
	/// Unit tests for the section service.
	/// </summary>
	public class SectionService_UnitTests
	{
		/// <summary>
		/// Ensures that GetSectionAsync returns the desired
		/// section, if it exists.
		/// </summary>
		[Fact]
		public async Task GetSectionAsync_Exists_ReturnSection()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			database.Reload();

			var sectionService = new SectionService(database.Context);
			var section = await sectionService.GetSectionAsync("Class1", "Section1");

			Assert.Equal("Class1", section.Classroom.Name);
			Assert.Equal("Section1", section.Name);
		}

		/// <summary>
		/// Ensures that GetSectionAsync returns null, if the desired
		/// section doesn't exist.
		/// </summary>
		[Fact]
		public async Task GetSectionAsync_DoesntExist_ReturnNull()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var sectionService = new SectionService(database.Context);
			var section = await sectionService.GetSectionAsync("Class1", "Section1");

			Assert.Null(section);
		}

		/// <summary>
		/// Ensures that GetSectionMembershipsAsync returns all section
		/// memberships in the given section.
		/// </summary>
		[Fact]
		public async Task GetSectionMembershipsAsync_ReturnStudents()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddSection("Class1", "Section2")
				.AddStudent("User1", "Last", "First", "Class1", "Section1")
				.AddStudent("User2", "Last", "First", "Class1", "Section1")
				.AddStudent("User3", "Last", "First", "Class1", "Section2")
				.Build();

			database.Reload();

			var sectionService = new SectionService(database.Context);
			var students = await sectionService.GetSectionStudentsAsync("Class1", "Section1");

			Assert.Equal(2, students.Count);
			Assert.Equal("User1", students[0].ClassroomMembership.User.UserName);
			Assert.Equal("User2", students[1].ClassroomMembership.User.UserName);
		}

		/// <summary>
		/// Ensures that CreateSectionAsync actually creates the section.
		/// </summary>
		[Fact]
		public async Task CreateSectionAsync_SectionCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var sectionService = new SectionService(database.Context);
			await sectionService.CreateSectionAsync
			(
				"Class1",
				new Section()
				{
					Name = "Section1"
				}
			);

			database.Reload();

			var section = database.Context.Sections
				.Include(qc => qc.Classroom)
				.Single();

			Assert.Equal("Class1", section.Classroom.Name);
			Assert.Equal("Section1", section.Name);
		}

		/// <summary>
		/// Ensures that UpdateSectionAsync actually updates the section.
		/// </summary>
		[Fact]
		public async Task UpdateSectionAsync_SectionUpdated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();

			var section = database.Context.Sections
				.Include(qc => qc.Classroom)
				.First();

			// Update the section
			database.Context.Entry(section).State = EntityState.Detached;
			section.DisplayName = "New Display Name";

			// Apply the update
			var sectionService = new SectionService(database.Context);
			await sectionService.UpdateSectionAsync("Class1", section);

			database.Reload();

			section = database.Context.Sections
				.Include(qc => qc.Classroom)
				.Single();

			Assert.Equal("Class1", section.Classroom.Name);
			Assert.Equal("Section1", section.Name);
			Assert.Equal("New Display Name", section.DisplayName);
		}

		/// <summary>
		/// Ensures that DeleteSectionAsync actually deletes a section.
		/// </summary>
		[Fact]
		public async Task DeleteSectionAsync_SectionDeleted()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.Build();
			
			database.Reload();

			var sectionService = new SectionService(database.Context);
			await sectionService.DeleteSectionAsync("Class1", "Section1");

			database.Reload();

			Assert.Equal(0, database.Context.Sections.Count());
		}
	}
}
