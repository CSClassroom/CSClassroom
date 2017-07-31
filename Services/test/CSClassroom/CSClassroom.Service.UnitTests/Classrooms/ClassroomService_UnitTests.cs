using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Classrooms
{
	/// <summary>
	/// Unit tests for the classroom service.
	/// </summary>
	public class ClassroomService_UnitTests
	{
		/// <summary>
		/// Ensures that GetClassroomsAsync returns all classrooms.
		/// </summary>
		[Fact]
		public async Task GetClassroomsAsync_ReturnsAllClassrooms()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.Build();

			var classroomService = GetClassroomService(database.Context);
			var classrooms = await classroomService.GetClassroomsAsync();

			Assert.Equal(2, classrooms.Count);
			Assert.Equal("Class1", classrooms[0].Name);
			Assert.Equal("Class1", classrooms[0].Name);
		}

		/// <summary>
		/// Ensures that GetClassroomAsync returns the desired
		/// classroom, if it exists.
		/// </summary>
		[Fact]
		public async Task GetClassroomAsync_Exists_ReturnsClassroom()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.Build();

			var classroomId = database.Context.Classrooms.First().Id;

			database.Reload();

			var classroomService = GetClassroomService(database.Context);
			var classroom = await classroomService.GetClassroomAsync("Class1");

			Assert.Equal("Class1", classroom.Name);
		}

		/// <summary>
		/// Ensures that GetClassroomAsync returns null, if the desired
		/// classroom doesn't exist.
		/// </summary>
		[Fact]
		public async Task GetClassroomAsync_DoesntExist_ReturnNull()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var classroomService = GetClassroomService(database.Context);
			var classroom = await classroomService.GetClassroomAsync("Class2");

			Assert.Null(classroom);
		}

		/// <summary>
		/// Ensures that GetClassroomsWithAccessAsync returns the classrooms
		/// the student has access to.
		/// </summary>
		[Fact]
		public async Task GetClassroomsWithAccessAsync_UserNotFound_ReturnsNull()
		{
			var database = new TestDatabaseBuilder().Build();

			var classroomService = GetClassroomService(database.Context);
			var classrooms = await classroomService.GetClassroomsWithAccessAsync(userId: 100);

			Assert.Equal(0, classrooms.Count);
		}

		/// <summary>
		/// Ensures that GetClassroomsWithAccessAsync returns the classrooms
		/// the student has access to.
		/// </summary>
		[Fact]
		public async Task GetClassroomsWithAccessAsync_Student_ReturnsClassrooms()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddClassroom("Class2")
				.AddSection("Class2", "Section2")
				.AddStudent("User1", "Last", "First", "Class2", "Section2")
				.Build();

			var userId = database.Context.Users.First().Id;
			var classroomId = database.Context.Classrooms
				.Single(c => c.Name == "Class2")
				.Id;

			database.Reload();

			var classroomService = GetClassroomService(database.Context);
			var classrooms = await classroomService.GetClassroomsWithAccessAsync(userId);

			Assert.Equal(1, classrooms.Count);
			Assert.Equal(userId, classrooms[0].UserId);
			Assert.Equal(classroomId, classrooms[0].ClassroomId);
		}

		/// <summary>
		/// Ensures that GetClassroomsWithAccessAsync returns the classrooms
		/// the classroom admin has access to.
		/// </summary>
		[Fact]
		public async Task GetClassroomsWithAccessAsync_Admin_ReturnsClassrooms()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddAdmin("User1", "Last", "First", "Class2", superUser: false)
				.Build();

			var userId = database.Context.Users.First().Id;
			var classroomId = database.Context.Classrooms
				.Single(c => c.Name == "Class2")
				.Id;

			database.Reload();

			var classroomService = GetClassroomService(database.Context);
			var classrooms = await classroomService.GetClassroomsWithAccessAsync(userId);

			Assert.Equal(1, classrooms.Count);
			Assert.Equal(userId, classrooms[0].UserId);
			Assert.Equal(classroomId, classrooms[0].ClassroomId);
		}

		/// <summary>
		/// Ensures that GetClassroomsWithAccessAsync returns the classrooms
		/// the system superuser has access to. This should be all classrooms.
		/// </summary>
		[Fact]
		public async Task GetClassroomsWithAccessAsync_SuperUser_ReturnsClassrooms()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddAdmin("User1", "Last", "First", "Class2", superUser: true)
				.Build();

			var userId = database.Context.Users.First().Id;
			var expectedClassrooms = database.Context.Classrooms
				.OrderBy(c => c.Id)
				.ToList();

			database.Reload();

			var classroomService = GetClassroomService(database.Context);
			var classrooms = await classroomService.GetClassroomsWithAccessAsync(userId);
			classrooms = classrooms.OrderBy(c => c.ClassroomId).ToList();

			Assert.Equal(2, classrooms.Count);
			Assert.Equal(userId, classrooms[0].UserId);
			Assert.Equal(expectedClassrooms[0].Id, classrooms[0].ClassroomId);
			Assert.Equal(userId, classrooms[1].UserId);
			Assert.Equal(expectedClassrooms[1].Id, classrooms[1].ClassroomId);
		}

		/// <summary>
		/// Ensures that GetClassroomAdminsAsync returns the admins
		/// of the given classroom.
		/// </summary>
		[Fact]
		public async Task GetClassroomAdminsAsync_ReturnsAdmins()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddAdmin("User1", "Last", "First", "Class1", superUser: false)
				.AddAdmin("User2", "Last", "First", "Class1", superUser: false)
				.AddAdmin("User3", "Last", "First", "Class2", superUser: false)
				.Build();

			var users = database.Context.Users
				.OrderBy(u => u.UserName)
				.ToList();

			database.Reload();

			var classroomService = GetClassroomService(database.Context);
			var admins = await classroomService.GetClassroomAdminsAsync("Class1");
			admins = admins.OrderBy(a => a.UserId).ToList();

			Assert.Equal(2, admins.Count);
			Assert.Equal(users[0].Id, admins[0].UserId);
			Assert.Equal(users[1].Id, admins[1].UserId);
		}

		/// <summary>
		/// Ensures that CreateClassroomAsync actually creates the classroom.
		/// </summary>
		[Fact]
		public async Task CreateClassroomAsync_ClassroomCreated()
		{
			var database = new TestDatabaseBuilder().Build();

			var classroomService = GetClassroomService(database.Context);
			await classroomService.CreateClassroomAsync
			(
				new Classroom()
				{
					Name = "Class1",
					ClassroomGradebooks = Collections.CreateList
					(
						new ClassroomGradebook() { Id = 1 },
						new ClassroomGradebook() { Id = 2 }
					)
				}
			);

			database.Reload();

			var classroom = database.Context
				.Classrooms
				.Include(c => c.ClassroomGradebooks)
				.Single();

			Assert.Equal("Class1", classroom.Name);
			Assert.Equal(2, classroom.ClassroomGradebooks.Count);
			Assert.Equal(0, classroom.ClassroomGradebooks[0].Order);
			Assert.Equal(1, classroom.ClassroomGradebooks[1].Order);
		}

		/// <summary>
		/// Ensures that UpdateClassroomAsync actually updates the classroom.
		/// </summary>
		[Fact]
		public async Task UpdateClassroomAsync_ClassroomUpdated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var classroom = database.Context.Classrooms.Single();

			// Update the classroom
			database.Context.Entry(classroom).State = EntityState.Detached;
			classroom.DisplayName = "New Display Name";

			// Apply the update
			var classroomService = GetClassroomService(database.Context);
			await classroomService.UpdateClassroomAsync(classroom);

			database.Reload();

			classroom = database.Context.Classrooms.Single();

			Assert.Equal("Class1", classroom.Name);
			Assert.Equal("New Display Name", classroom.DisplayName);
		}

		/// <summary>
		/// Ensures that ArchiveClassroomAsync returns false when the
		/// classroom doesn't exist.
		/// </summary>
		[Fact]
		public async Task ArchiveClassroomAsync_ClassroomNotFound_ReturnsFalse()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var classroomService = GetClassroomService(database.Context);

			var result = await classroomService.ArchiveClassroomAsync
			(
				"Class2", 
				"Class2-Archived"
			);

			Assert.False(result);
		}

		/// <summary>
		/// Ensures that ArchiveClassroomAsync returns false when the
		/// classroom doesn't exist.
		/// </summary>
		[Fact]
		public async Task ArchiveClassroomAsync_ClassrooFound_ReturnsTrueAndArchivesClassroom()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var mockArchiver = new Mock<IClassroomArchiver>();
			mockArchiver
				.Setup(a => a.ArchiveClassroomAsync("Class1", "Class1A"))
				.Returns(Task.CompletedTask);

			var classroomService = GetClassroomService
			(
				database.Context,
				mockArchiver.Object
			);

			var result = await classroomService.ArchiveClassroomAsync
			(
				"Class1",
				"Class1A"
			);

			Assert.True(result);
			mockArchiver.Verify(a => a.ArchiveClassroomAsync("Class1", "Class1A"));
		}

		/// <summary>
		/// Ensures that DeleteClassroomAsync actually deletes a classroom.
		/// </summary>
		[Fact]
		public async Task DeleteClassroomAsync_ClassroomDeleted()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var classroomId = database.Context.Classrooms.First().Id;

			database.Reload();

			var classroomService = GetClassroomService(database.Context);
			await classroomService.DeleteClassroomAsync("Class1");

			database.Reload();

			Assert.Equal(0, database.Context.Classrooms.Count());
		}

		/// <summary>
		/// Returns a new ClassroomService object.
		/// </summary>
		private ClassroomService GetClassroomService(
			DatabaseContext dbContext,
			IClassroomArchiver archiver = null)
		{
			return new ClassroomService(dbContext, archiver);
		}
	}
}
