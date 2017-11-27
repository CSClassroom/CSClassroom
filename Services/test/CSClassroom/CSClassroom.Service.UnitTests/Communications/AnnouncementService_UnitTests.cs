using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.Security;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Communications;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Communications
{
	/// <summary>
	/// Unit tests for the AnnouncementService class.
	/// </summary>
	public class AnnouncementService_UnitTests
	{
		private readonly DateTime PostDate1 = new DateTime(2017, 1, 1);
		private readonly DateTime PostDate2 = new DateTime(2017, 1, 2);
		private readonly DateTime PostDate3 = new DateTime(2017, 1, 3);

		/// <summary>
		/// Ensures that GetAssignmentsAsync returns the correct assignments for
		/// a student.
		/// </summary>
		[Fact]
		public async Task GetAnnouncementsAsync_UserIsStudent_ReturnSectionAnnouncements()
		{
			var database = GetDatabaseWithAnnouncements().Build();
			int userId = database.Context.Users.Single(u => u.UserName == "Student1").Id;
			database.Reload();

			var announcementService = GetAnnouncementService(database.Context);

			var announcementsQuery = await announcementService
				.GetAnnouncementsAsync("Class1", userId, admin: false);
			var announcements = announcementsQuery.OrderBy(a => a.DatePosted).ToList();

			Assert.Equal(2, announcements.Count);

			Assert.Equal("Title1", announcements[0].Title);
			Assert.Equal("Contents1", announcements[0].Contents);
			Assert.Equal("Admin1", announcements[0].User.UserName);
			Assert.Equal(PostDate1, announcements[0].DatePosted);

			Assert.Equal("Title2", announcements[1].Title);
			Assert.Equal("Contents2", announcements[1].Contents);
			Assert.Equal("Admin1", announcements[1].User.UserName);
			Assert.Equal(PostDate2, announcements[1].DatePosted);
		}

		/// <summary>
		/// Ensures that GetAssignmentsAsync returns the correct assignments for
		/// a student.
		/// </summary>
		[Fact]
		public async Task GetAnnouncementAsync_NoSuchAnnouncement_ReturnsNull()
		{
			var database = GetDatabase().Build();

			var announcementService = GetAnnouncementService(database.Context);
			var announcement = await announcementService.GetAnnouncementAsync
			(
				"Class1",
				announcementId: 12345
			);

			Assert.Null(announcement);
		}

		/// <summary>
		/// Ensures that GetAssignmentsAsync returns the correct assignments for
		/// a student.
		/// </summary>
		[Fact]
		public async Task GetAnnouncementAsync_ValidAnnouncement_ReturnsAnnouncement()
		{
			var database = GetDatabaseWithAnnouncements().Build();
			int announcementId = database.Context
				.Announcements
				.Single(a => a.DatePosted == PostDate2)
				.Id;
			database.Reload();

			var announcementService = GetAnnouncementService(database.Context);

			var announcement = await announcementService.GetAnnouncementAsync
			(
				"Class1", 
				announcementId
			);

			Assert.NotNull(announcement);
			Assert.Equal("Title2", announcement.Title);
			Assert.Equal("Contents2", announcement.Contents);
			Assert.Equal("Admin1", announcement.User.UserName);
			Assert.Equal(PostDate2, announcement.DatePosted);
			Assert.Equal(1, announcement.Sections.Count);
			Assert.Equal("Section1", announcement.Sections[0].Section.Name);
		}

		/// <summary>
		/// Ensures that GetAssignmentsAsync returns the correct assignments for
		/// an admin.
		/// </summary>
		[Fact]
		public async Task GetAnnouncementsAsync_UserIsAdmin_ReturnClassAnnouncements()
		{
			var database = GetDatabaseWithAnnouncements().Build();
			int userId = database.Context.Users.Single(u => u.UserName == "Admin1").Id;
			database.Reload();

			var announcementService = GetAnnouncementService(database.Context);

			var announcementsQuery = await announcementService
				.GetAnnouncementsAsync("Class1", userId, admin: true);
			var announcements = announcementsQuery.OrderBy(a => a.DatePosted).ToList();

			Assert.Equal(3, announcements.Count);

			Assert.Equal("Title1", announcements[0].Title);
			Assert.Equal("Contents1", announcements[0].Contents);
			Assert.Equal("Admin1", announcements[0].User.UserName);
			Assert.Equal(PostDate1, announcements[0].DatePosted);
			Assert.Equal(2, announcements[0].Sections.Count);
			Assert.Contains(announcements[0].Sections, s => s.Section.Name == "Section1");
			Assert.Contains(announcements[0].Sections, s => s.Section.Name == "Section2");

			Assert.Equal("Title2", announcements[1].Title);
			Assert.Equal("Contents2", announcements[1].Contents);
			Assert.Equal("Admin1", announcements[1].User.UserName);
			Assert.Equal(PostDate2, announcements[1].DatePosted);
			Assert.Equal(1, announcements[1].Sections.Count);
			Assert.Contains(announcements[1].Sections, s => s.Section.Name == "Section1");

			Assert.Equal("Title3", announcements[2].Title);
			Assert.Equal("Contents3", announcements[2].Contents);
			Assert.Equal("Admin1", announcements[2].User.UserName);
			Assert.Equal(PostDate3, announcements[2].DatePosted);
			Assert.Equal(1, announcements[2].Sections.Count);
			Assert.Contains(announcements[2].Sections, s => s.Section.Name == "Section2");
		}

		/// <summary>
		/// Ensures that PostAnnouncementAsync errors out if validation failed.
		/// </summary>
		[Fact]
		public async Task PostAnnouncementAsync_ValidationFailed_ReturnsFalseWithError()
		{
			var database = GetDatabase().Build();
			var classroomId = database.Context.Classrooms.First().Id;
			var userId = database.Context
				.Users
				.Single(u => u.UserName == "Admin1")
				.Id;

			var announcement = new Announcement()
			{
				Sections = new List<AnnouncementSection>()
			};

			var validator = GetMockValidator(classroomId, announcement, result: false);
			var modelErrors = new MockErrorCollection();
			var announcementService = GetAnnouncementService(database.Context, validator);
			var result = await announcementService.PostAnnouncementAsync
			(
				"Class1",
				userId,
				announcement,
				dt => string.Empty,
				modelErrors
			);

			database.Reload();

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Error"));
			Assert.False(database.Context.Announcements.Any());
		}

		/// <summary>
		/// Ensures that PostAnnouncementAsync posts the announcement if validation
		/// was successful.
		/// </summary>
		[Fact]
		public async Task PostAnnouncementAsync_ValidationSucceeded_AnnouncementPosted()
		{
			var database = GetDatabase().Build();
			var classroomId = database.Context.Classrooms.First().Id;
			var userId = database.Context
				.Users
				.Single(u => u.UserName == "Admin1")
				.Id;
			var sectionId = database.Context
				.Sections
				.Single(s => s.Name == "Section1")
				.Id;
			database.Reload();

			var announcement = new Announcement()
			{
				Title = "Title",
				Contents = "Contents",
				Sections = Collections.CreateList
				(
					new AnnouncementSection() {SectionId = sectionId}
				)
			};

			var validator = GetMockValidator(classroomId, announcement, result: true);

			var htmlSanitizer = new Mock<IHtmlSanitizer>();
			htmlSanitizer
				.Setup(h => h.SanitizeHtml("Contents"))
				.Returns("SanitizedContents");

			var emailProvider = GetMockEmailProvider
			(
				Collections.CreateList("Student1Email", "Admin1Email", "AdditionalContact1Email"),
				"Class1 DisplayName: Title",
				"SanitizedContents"
			);

			var timeProvider = new Mock<ITimeProvider>();
			timeProvider.Setup(t => t.UtcNow).Returns(PostDate1);

			var announcementService = GetAnnouncementService
			(
				database.Context,
				validator,
				htmlSanitizer.Object,
				emailProvider.Object,
				timeProvider.Object
			);

			var modelErrors = new MockErrorCollection();
			var result = await announcementService.PostAnnouncementAsync
			(
				"Class1",
				userId,
				announcement,
				dt => string.Empty,
				modelErrors
			);

			database.Reload();
			announcement = database.Context
				.Announcements
				.Include(a => a.Sections)
				.Single();

			Assert.True(result);
			Assert.Equal(userId, announcement.UserId);
			Assert.Equal("Title", announcement.Title);
			Assert.Equal("SanitizedContents", announcement.Contents);
			Assert.Equal(PostDate1, announcement.DatePosted);
			Assert.Equal(1, announcement.Sections.Count);
			Assert.Equal(sectionId, announcement.Sections[0].SectionId);
			emailProvider.VerifyAll();
		}

		/// <summary>
		/// Ensures that EditAnnouncementAsync errors out if validation failed.
		/// </summary>
		[Fact]
		public async Task EditAnnouncementAsync_ValidationFailed_ReturnsFalseWithError()
		{
			var database = GetDatabaseWithAnnouncements().Build();
			var classroomId = database.Context.Classrooms.First().Id;
			var announcement = database.Context
				.Announcements
				.Single(a => a.DatePosted == PostDate2);

			var validator = GetMockValidator(classroomId, announcement, result: false);
			var modelErrors = new MockErrorCollection();
			var announcementService = GetAnnouncementService(database.Context, validator);

			announcement.Title = "UpdatedTitle";
			var result = await announcementService.EditAnnouncementAsync
			(
				"Class1",
				announcement,
				dt => string.Empty,
				modelErrors
			);

			database.Reload();

			announcement = database.Context
				.Announcements
				.Single(a => a.DatePosted == PostDate2);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Error"));
			Assert.Equal("Title2", announcement.Title);
		}

		/// <summary>
		/// Ensures that PostAnnouncementAsync posts the announcement.
		/// </summary>
		[Fact]
		public async Task EditAnnouncementAsync_ValidationSucceeded_AnnouncementEdited()
		{
			var database = GetDatabaseWithAnnouncements().Build();
			var announcement = database.Context
				.Announcements
				.Include(a => a.Classroom.Sections)
				.Include(a => a.Sections)
				.Single(a => a.DatePosted == PostDate2);

			var classroomId = database.Context.Classrooms.First().Id;
			var userId = database.Context
				.Users
				.Single(u => u.UserName == "Admin1")
				.Id;
			var sectionId = database.Context
				.Sections
				.Single(s => s.Name == "Section2")
				.Id;
			database.Reload();

			var validator = GetMockValidator(classroomId, announcement, result: true);
			var htmlSanitizer = new Mock<IHtmlSanitizer>();
			htmlSanitizer
				.Setup(h => h.SanitizeHtml("UpdatedContents"))
				.Returns("SanitizedUpdatedContents");

			var emailProvider = GetMockEmailProvider
			(
				Collections.CreateList("Student2Email", "AdditionalContact2Email"),
				"Class1 DisplayName: UpdatedTitle",
				"SanitizedUpdatedContents"
			);

			var timeProvider = new Mock<ITimeProvider>();
			timeProvider.Setup(t => t.UtcNow).Returns(PostDate1);

			var announcementService = GetAnnouncementService
			(
				database.Context,
				validator,
				htmlSanitizer.Object,
				emailProvider.Object,
				timeProvider.Object
			);

			var modelErrors = new MockErrorCollection();

			announcement.Title = "UpdatedTitle";
			announcement.Contents = "UpdatedContents";
			announcement.Sections.Add
			(
				new AnnouncementSection()
				{
					SectionId = sectionId
				}
			);

			var result = await announcementService.EditAnnouncementAsync
			(
				"Class1",
				announcement,
				dt => string.Empty,
				modelErrors
			);

			database.Reload();
			announcement = database.Context
				.Announcements
				.Include(a => a.Sections)
				.ThenInclude(s => s.Section)
				.Single(a => a.DatePosted == PostDate2);
			var sections = announcement.Sections
				.OrderBy(s => s.Section.Name)
				.ToList();

			Assert.True(result);
			Assert.Equal(userId, announcement.UserId);
			Assert.Equal("UpdatedTitle", announcement.Title);
			Assert.Equal("SanitizedUpdatedContents", announcement.Contents);
			Assert.Equal(PostDate2, announcement.DatePosted);
			Assert.Equal(2, announcement.Sections.Count);
			Assert.Equal("Section1", sections[0].Section.Name);
			Assert.Equal("Section2", sections[1].Section.Name);
			emailProvider.VerifyAll();
		}

		/// <summary>
		/// Ensures that DeleteAnnouncementAsync deletes an assignment.
		/// </summary>
		[Fact]
		public async Task DeleteAnnouncementAsync_AnnouncementDeleted()
		{
			var database = GetDatabaseWithAnnouncements().Build();
			var announcement = database.Context
				.Announcements
				.Single(a => a.DatePosted == PostDate2);

			var announcementService = GetAnnouncementService(database.Context);
			
			await announcementService.DeleteAnnouncementAsync
			(
				"Class1",
				announcement.Id
			);

			database.Reload();

			announcement = database.Context
				.Announcements
				.SingleOrDefault(a => a.DatePosted == PostDate2);

			Assert.Null(announcement);
		}

		/// <summary>
		/// Returns a database builder.
		/// </summary>
		/// <returns></returns>
		private TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Section1")
				.AddSection("Class1", "Section2")
				.AddAdmin("Admin1", "LastName", "FirstName", "Class1", superUser: false)
				.AddStudent("Student1", "LastName", "FirstName", "Class1", "Section1")
				.AddStudent("Student2", "LastName", "FirstName", "Class1", "Section2")
				.AddAdditionalContact("Student1", "LastName", "FirstName", "AdditionalContact1Email")
				.AddAdditionalContact("Student2", "LastName", "FirstName", "AdditionalContact2Email");
		}

		/// <summary>
		/// Returns a database builder with pre-added announcements.
		/// </summary>
		private TestDatabaseBuilder GetDatabaseWithAnnouncements()
		{
			return GetDatabase()
				.AddAnnouncement("Class1", "Admin1", "Title1", "Contents1", PostDate1, Collections.CreateList("Section1", "Section2"))
				.AddAnnouncement("Class1", "Admin1", "Title2", "Contents2", PostDate2, Collections.CreateList("Section1"))
				.AddAnnouncement("Class1", "Admin1", "Title3", "Contents3", PostDate3, Collections.CreateList("Section2"));
		}

		/// <summary>
		/// Returns a mock announcement validator.
		/// </summary>
		private IAnnouncementValidator GetMockValidator(
			int classroomId,
			Announcement announcement,
			bool result)
		{
			var validator = new Mock<IAnnouncementValidator>();
			validator
				.Setup
				(
					v => v.ValidateAnnouncement
					(
						It.Is<Classroom>(c => c.Id == classroomId && c.Sections != null),
						announcement,
						It.IsAny<IModelErrorCollection>()
					)
				)
				.Callback<Classroom, Announcement, IModelErrorCollection>
				(
					(_1, _2, errors) =>
					{
						if (result)
						{
							errors.AddError("Error", "Error message");
						}
					}
				).Returns(result);

			return validator.Object;
		}

		/// <summary>
		/// Returns a mock e-mail provider.
		/// </summary>
		private static Mock<IEmailProvider> GetMockEmailProvider(
			ICollection<string> expectedEmails,
			string subject,
			string message)
		{
			var emailProvider = new Mock<IEmailProvider>(MockBehavior.Strict);
			emailProvider
				.Setup
				(
					e => e.SendMessageAsync
					(
						It.Is<IList<EmailRecipient>>
						(
							to => to.Select(r => r.EmailAddress).ToHashSet().SetEquals(expectedEmails)
						),
						subject,
						message
					)
				).Returns(Task.CompletedTask);
			return emailProvider;
		}

		/// <summary>
		/// Returns a new announcement service.
		/// </summary>
		private AnnouncementService GetAnnouncementService(
			DatabaseContext dbContext,
			IAnnouncementValidator validator = null,
			IHtmlSanitizer htmlSanitizer = null,
			IEmailProvider emailProvider = null,
			ITimeProvider timeProvider = null)
		{
			return new AnnouncementService
			(
				dbContext,
				validator,
				htmlSanitizer, 
				emailProvider, 
				timeProvider
			);
		}
	}
}
