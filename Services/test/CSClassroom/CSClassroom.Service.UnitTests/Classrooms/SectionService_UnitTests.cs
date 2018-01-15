using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
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

			var sectionService = CreateSectionService(database.Context);
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

			var sectionService = CreateSectionService(database.Context);
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

			var sectionService = CreateSectionService(database.Context);
			var students = await sectionService.GetSectionStudentsAsync("Class1", "Section1");

			Assert.Equal(2, students.Count);
			Assert.Equal("User1", students[0].ClassroomMembership.User.UserName);
			Assert.Equal("User2", students[1].ClassroomMembership.User.UserName);
		}

		/// <summary>
		/// Ensures that CreateSectionAsync does not create an invalid section.
		/// </summary>
		[Fact]
		public async Task CreateSectionAsync_InvalidSection_SectionNotCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var section = new Section() { Name = "Section1" };
			var errors = new MockErrorCollection();
			var validator = CreateMockSectionValidator(section, errors, valid: false);
			var sectionService = CreateSectionService(database.Context, validator);
			
			var result = await sectionService.CreateSectionAsync("Class1", section, errors);

			database.Reload();

			section = database.Context.Sections
				.Include(qc => qc.Classroom)
				.SingleOrDefault();

			Assert.False(result);
			Assert.True(errors.HasErrors);
			Assert.Null(section);
		}

		/// <summary>
		/// Ensures that CreateSectionAsync actually creates a section.
		/// </summary>
		[Fact]
		public async Task CreateSectionAsync_ValidSection_SectionCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var section = new Section() { Name = "Section1" };
			var errors = new MockErrorCollection();
			var validator = CreateMockSectionValidator(section, errors, valid: true);
			var sectionService = CreateSectionService(database.Context, validator);
			
			var result = await sectionService.CreateSectionAsync("Class1", section, errors);

			database.Reload();

			section = database.Context.Sections
				.Include(qc => qc.Classroom)
				.Single();

			Assert.True(result);
			Assert.False(errors.HasErrors);
			Assert.Equal("Class1", section.Classroom.Name);
			Assert.Equal("Section1", section.Name);
		}

		/// <summary>
		/// Ensures that CreateSectionAsync does not update a section with invalid changes.
		/// </summary>
		[Fact]
		public async Task UpdateSectionAsync_InvalidSection_SectionNotUpdated()
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
			var errors = new MockErrorCollection();
			var validator = CreateMockSectionValidator(section, errors, valid: false);
			var sectionService = CreateSectionService(database.Context, validator);
			var result = await sectionService.UpdateSectionAsync("Class1", section, errors);

			database.Reload();

			section = database.Context.Sections
				.Include(qc => qc.Classroom)
				.Single();

			Assert.False(result);
			Assert.True(errors.HasErrors);
			Assert.NotEqual("New Display Name", section.Name);
		}

		/// <summary>
		/// Ensures that UpdateSectionAsync actually updates a section.
		/// </summary>
		[Fact]
		public async Task UpdateSectionAsync_ValidSection_SectionUpdated()
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
			var errors = new MockErrorCollection();
			var validator = CreateMockSectionValidator(section, errors, valid: true);
			var sectionService = CreateSectionService(database.Context, validator);
			var result = await sectionService.UpdateSectionAsync("Class1", section, errors);

			database.Reload();

			section = database.Context.Sections
				.Include(qc => qc.Classroom)
				.Single();

			Assert.True(result);
			Assert.False(errors.HasErrors);
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

			var sectionService = CreateSectionService(database.Context);
			await sectionService.DeleteSectionAsync("Class1", "Section1");

			database.Reload();

			Assert.Equal(0, database.Context.Sections.Count());
		}

		/// <summary>
		/// Returns a new section validator.
		/// </summary>
		private static ISectionValidator CreateMockSectionValidator(
			Section sectionToValidate,
			IModelErrorCollection modelErrors,
			bool valid)
		{
			var sectionValidator = new Mock<ISectionValidator>();
			sectionValidator
				.Setup
				(
					m => m.ValidateSectionAsync
					(
						sectionToValidate, 
						modelErrors
					)
				)
				.Callback
				(
					(Section section, IModelErrorCollection errors) =>
					{
						if (!valid)
						{
							errors.AddError("Error", "Error Description");
						}
					} 
				).ReturnsAsync(valid);

			return sectionValidator.Object;
		}

		/// <summary>
		/// Creates a section service.
		/// </summary>
		private SectionService CreateSectionService(
			DatabaseContext dbContext,
			ISectionValidator sectionValidator = null)
		{
			return new SectionService(dbContext, sectionValidator);
		}
	}
}
