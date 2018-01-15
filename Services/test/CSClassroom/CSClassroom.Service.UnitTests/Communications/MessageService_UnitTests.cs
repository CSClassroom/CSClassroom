using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.Image;
using CSC.Common.Infrastructure.Security;
using CSC.Common.Infrastructure.System;
using CSC.Common.TestDoubles;
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
	/// Unit tests for the MessageService class.
	/// </summary>
	public class MessageService_UnitTests
	{
		/// <summary>
		/// A date that a message was sent.
		/// </summary>
		private static readonly DateTime SentDate = new DateTime(2017, 1, 1);

		/// <summary>
		/// Returns student test cases for GetConversationsAsync/GetConversationAsync.
		/// </summary>
		public static IEnumerable<object[]> GetConversationStudentTestCases()
		{
			// Sent from self to teacher
			yield return new object[]
			{
				"Student1" /* creator */,
				"Student1" /* student */,
				false /* draft */,
				true /* expectedToInclude */
			};
			
			// Sent to self from teacher
			yield return new object[]
			{
				"Teacher1" /* creator */,
				"Student1" /* student */,
				false /* draft */,
				true /* expectedToInclude */
			};

			// Draft from self to teacher
			yield return new object[]
			{
				"Student1" /* creator */,
				"Student1" /* student */, 
				true /* draft */,
				true /* expectedToInclude */
			};

			// Sent from another student
			yield return new object[]
			{
				"Student2" /* creator */,
				"Student2" /* student */, 
				false /* draft */,
				false /* expectedToInclude */
			};

			// Sent to another student
			yield return new object[]
			{
				"Teacher1" /* creator */,
				"Student2" /* student */, 
				false /* draft */,
				false /* expectedToInclude */
			};

			// Draft to self from teacher
			yield return new object[]
			{
				"Teacher1" /* creator */,
				"Student1" /* student */, 
				true /* draft */,
				false /* expectedToInclude */
			};
		}

		/// <summary>
		/// Returns admin test cases for GetConversationsAsync/GetConversationAsync.
		/// </summary>
		public static IEnumerable<object[]> GetConversationTeacherTestCases()
		{
			// Sent from self to student in section
			yield return new object[]
			{
				"Teacher1" /* creator */,
				"Student1" /* student */, 
				false /* draft */,
				true /* expectedToInclude */
			};
			
			// Sent from another admin to student in section
			yield return new object[]
			{
				"Teacher2" /* creator */,
				"Student1" /* student */, 
				false /* draft */,
				true /* expectedToInclude */
			};
			
			// Draft from self to student in section
			yield return new object[]
			{
				"Teacher1" /* creator */,
				"Student1" /* student */, 
				true /* draft */,
				true /* expectedToInclude */
			};
			
			// Sent from student in section
			yield return new object[]
			{
				"Student1" /* creator */,
				"Student1" /* student */, 
				false /* draft */,
				true /* expectedToInclude */
			};
			
			// Sent from student out of section
			yield return new object[]
			{
				"Student3" /* creator */,
				"Student3" /* student */, 
				false /* draft */,
				false /* expectedToInclude */
			};
			
			// Sent from another admin to student out of section
			yield return new object[]
			{
				"Teacher2" /* creator */,
				"Student3" /* student */, 
				false /* draft */,
				false /* expectedToInclude */
			};
			
			// Draft from another admin to student in section
			yield return new object[]
			{
				"Teacher2" /* creator */,
				"Student1" /* student */, 
				true /* draft */,
				false /* expectedToInclude */
			};
			
			// Draft from student in section
			yield return new object[]
			{
				"Student1" /* creator */,
				"Student1" /* student */, 
				true /* draft */,
				false /* expectedToInclude */
			};
			
			// Draft from student out of section
			yield return new object[]
			{
				"Student3" /* creator */,
				"Student3" /* student */,
				true /* draft */,
				false /* expectedToInclude */
			};
		}
		
		/// <summary>
		/// Ensures that GetConversationsAsync returns the correct conversations for
		/// a student.
		/// </summary>
		[Theory]
		[MemberData(nameof(GetConversationStudentTestCases))]	
		public async Task GetConversationsAsync_Student_ReturnsCorrectConversations(
			string creator,
			string student,
			bool draft,
			bool expectedToInclude)
		{
			var database = GetDatabase()
				.AddConversation("Class1", student, creator, "Subject", GetSentDate(draft))
				.Build();

			var studentId = GetUserId(database.Context, "Student1");
			database.Reload();

			var messageService = CreateMessageService(database.Context);

			var query = await messageService.GetConversationsAsync
			(
				"Class1", 
				studentId /* userId */,
				null /* studentId */,
				admin: false
			);

			var results = await query.ToListAsync();

			if (expectedToInclude)
			{
				Assert.Single(results);
			}
			else
			{
				Assert.Empty(results);
			}
		}
		
		/// <summary>
		/// Ensures that GetConversationsAsync returns the correct conversations for
		/// an admin.
		/// </summary>
		[Theory]
		[MemberData(nameof(GetConversationTeacherTestCases))]	
		public async Task GetConversationsAsync_Teacher_ReturnsCorrectConversations(
			string creator,
			string student,
			bool draft,
			bool expectedToInclude)
		{
			var database = GetDatabase()
				.AddConversation("Class1", student, creator, "Subject", GetSentDate(draft))
				.Build();

			var teacherId = GetUserId(database.Context, "Teacher1");
			database.Reload();

			var messageService = CreateMessageService(database.Context);

			var query = await messageService.GetConversationsAsync
			(
				"Class1", 
				teacherId /* userId */,
				null /* studentId */,
				admin: true
			);

			var results = await query.ToListAsync();

			if (expectedToInclude)
			{
				Assert.Single(results);
			}
			else
			{
				Assert.Empty(results);
			}
		}
		
		/// <summary>
		/// Ensures that GetConversationAsync returns the given conversation if the
		/// student has permission, or null otherwise.
		/// </summary>
		[Theory]
		[MemberData(nameof(GetConversationStudentTestCases))]	
		public async Task GetConversationAsync_Student_ReturnsConversationIfHasPermission(
			string creator,
			string student,
			bool draft,
			bool expectedToReturn)
		{
			var database = GetDatabase()
				.AddConversation("Class1", student, creator, "Subject", GetSentDate(draft))
				.Build();

			var studentId = GetUserId(database.Context, "Student1");
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			var result = await messageService.GetConversationAsync
			(
				"Class1", 
				database.Context.Conversations.Single().Id,
				studentId,
				admin: false
			);

			if (expectedToReturn)
			{
				Assert.NotNull(result);
			}
			else
			{
				Assert.Null(result);
			}
		}
		
		/// <summary>
		/// Ensures that GetConversationAsync returns the given conversation if the
		/// teacher has permission, or null otherwise.
		/// </summary>
		[Theory]
		[MemberData(nameof(GetConversationTeacherTestCases))]	
		public async Task GetConversationAsync_Teacher_ReturnsConversationIfHasPermission(
			string creator,
			string student,
			bool draft,
			bool expectedToReturn)
		{
			var database = GetDatabase()
				.AddConversation("Class1", student, creator, "Subject", GetSentDate(draft))
				.Build();

			var teacherId = GetUserId(database.Context, "Teacher1");
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			var result = await messageService.GetConversationAsync
			(
				"Class1", 
				database.Context.Conversations.Single().Id,
				teacherId, 
				admin: true
			);

			if (expectedToReturn)
			{
				Assert.NotNull(result);
			}
			else
			{
				Assert.Null(result);
			}
		}

		/// <summary>
		/// Ensures that GetAttachmentAsync returns an attachment in a conversation.
		/// </summary>
		[Fact]
		public async Task GetAttachmentAsync_ReturnsAttachment()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate, 
					Collections.CreateList
					(
						new KeyValuePair<string, byte[]>("Attachment.jpg", new byte[5])
					))
				.Build();

			var studentId = GetUserId(database.Context, "Student1");
			var conversationId = database.Context.Conversations.Single().Id;
			var attachmentId = database.Context.Attachments.Single().Id;
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			var result = await messageService.GetAttachmentAsync
			(
				"Class1",
				conversationId,
				studentId,
				false /* admin */,
				attachmentId
			);
			
			Assert.Equal("Attachment.jpg", result.FileName);
			Assert.Equal(5, result.AttachmentData.FileContents.Length);
		}

		/// <summary>
		/// Ensures that GetStudentListAsync returns a list of all students that
		/// can receive messages from the current admin.
		/// </summary>
		[Fact]
		public async Task GetStudentListAsync_ReturnsStudentList()
		{
			var database = GetDatabase().Build();

			var adminId = GetUserId(database.Context, "Teacher1");
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			var results = await messageService.GetStudentListAsync
			(
				"Class1",
				adminId
			);
			
			Assert.Equal(2, results.Count);
			
			Assert.Equal("Last1", results[0].User.LastName);
			Assert.Equal("First1", results[0].User.FirstName);
			Assert.Equal("Period1", results[0].SectionMemberships[0].Section.Name);
			
			Assert.Equal("Last2", results[1].User.LastName);
			Assert.Equal("First2", results[1].User.FirstName);
			Assert.Equal("Period1", results[1].SectionMemberships[0].Section.Name);
		}

		/// <summary>
		/// Ensures that CreateConversationAsync actually creates a new
		/// conversation.
		/// </summary>
		[Fact]
		public async Task CreateConversationAsync_CreatesConversation()
		{
			var database = GetDatabase().Build();

			var studentId = GetUserId(database.Context, "Student1");
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			var result = await messageService.CreateConversationAsync
			(
				"Class1",
				studentId,
				studentId,
				"Subject"
			);

			database.Reload();
			result = database.Context.Conversations
				.Include(c => c.Messages)
				.Single(c => c.Id == result.Id);

			Assert.Equal(studentId, result.CreatorId);
			Assert.Equal(studentId, result.StudentId);
			Assert.Null(result.OwnerId);
			Assert.False(result.Actionable);
			Assert.Equal("Subject", result.Subject);
			Assert.Single(result.Messages);
			Assert.Equal(studentId, result.Messages[0].AuthorId);
		}

		/// <summary>
		/// Ensures that UpdateConversationOwnerAsync does not update the owner
		/// when the expected owner passed in is incorrect.
		/// </summary>
		[Fact]
		public async Task UpdateConversationOwnerAsync_ExpectedOwnerIncorrect_OwnerNotUpdated()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate)
				.Build();
			
			var teacher1Id = GetUserId(database.Context, "Teacher1");
			var teacher2Id = GetUserId(database.Context, "Teacher2");
			var conversation = database.Context.Conversations.Single();
			var conversationId = database.Context.Conversations.Single().Id;
			conversation.OwnerId = teacher2Id;
			database.Context.Update(conversation);
			database.Context.SaveChanges();
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			var result = await messageService.UpdateConversationOwnerAsync
			(
				"Class1",
				conversationId,
				teacher1Id,
				expectedOwnerId: null,
				newOwnerId: teacher1Id
			);

			database.Reload();

			var newOwnerId = database.Context.Conversations.Single().OwnerId;
			
			Assert.Equal(teacher2Id, newOwnerId);
			Assert.Equal(teacher2Id, result.Id);
		}

		/// <summary>
		/// Ensures that UpdateConversationOwnerAsync updates the owner correctly
		/// when the given expected owner is correct.
		/// </summary>
		[Fact]
		public async Task UpdateConversationOwnerAsync_ExpectedOwnerCorrect_UpdatesOwner()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate)
				.Build();
			
			var teacherId = GetUserId(database.Context, "Teacher1");
			var conversationId = database.Context.Conversations.Single().Id;
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			var result = await messageService.UpdateConversationOwnerAsync
			(
				"Class1",
				conversationId,
				teacherId,
				expectedOwnerId: null,
				newOwnerId: teacherId
			);

			database.Reload();

			var newOwnerId = database.Context.Conversations.Single().OwnerId;
			
			Assert.Equal(teacherId, newOwnerId);
			Assert.Equal(teacherId, result.Id);
		}

		/// <summary>
		/// Ensures that UpdateConversationStatusAsync updates conversation status.
		/// </summary>
		[Fact]
		public async Task UpdateConversationStatusAsync_UpdatesStatus()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate)
				.Build();
			
			var teacherId = GetUserId(database.Context, "Teacher1");
			var conversationId = database.Context.Conversations.Single().Id;
			database.Reload();
			
			var messageService = CreateMessageService(database.Context);

			await messageService.UpdateConversationStatusAsync
			(
				"Class1",
				conversationId,
				teacherId,
				actionRequired: true
			);

			database.Reload();
			
			Assert.True(database.Context.Conversations.Single().Actionable);
		}

		/// <summary>
		/// Ensures that SaveMessageDraftAsync creates a new draft message if none exists.
		/// </summary>
		[Fact]
		public async Task SaveMessageDraftAsync_NoDraftMessage_CreatesDraftMessage()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate)
				.Build();
			
			var userId = GetUserId(database.Context, "Student1");
			var conversationId = database.Context.Conversations.Single().Id;
			database.Reload();

			var htmlSanitizer = GetMockHtmlSanitizer("New", "Sanitized");
			var messageService = CreateMessageService
			(
				database.Context, 
				htmlSanitizer: htmlSanitizer.Object
			);

			await messageService.SaveMessageDraftAsync
			(
				"Class1", 
				conversationId, 
				userId, 
				false /* admin */, 
				"New"
			);

			database.Reload();
			var conversation = database.Context
				.Conversations
				.Include(c => c.Messages)
				.Single(c => c.Id == conversationId);
			var message = conversation.Messages
				.SingleOrDefault(m => m.AuthorId == userId && !m.Sent.HasValue);
			
			Assert.NotNull(message);
			Assert.Equal("Sanitized", message.Contents);
		}

		/// <summary>
		/// Ensures that SaveMessageDraftAsync updates an existing draft message if one exists.
		/// </summary>
		[Fact]
		public async Task SaveMessageDraftAsync_ExistingDraftMessage_UpdatesMessage()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", sent: null)
				.Build();
			
			var userId = GetUserId(database.Context, "Student1");
			var conversationId = database.Context.Conversations.Single().Id;
			database.Reload();

			var htmlSanitizer = GetMockHtmlSanitizer("Updated", "Sanitized");
			var messageService = CreateMessageService
			(
				database.Context, 
				htmlSanitizer: htmlSanitizer.Object
			);

			await messageService.SaveMessageDraftAsync
			(
				"Class1", 
				conversationId, 
				userId, 
				false /* admin */, 
				"Updated"
			);

			database.Reload();
			var conversation = database.Context
				.Conversations
				.Include(c => c.Messages)
				.Single(c => c.Id == conversationId);
			var message = conversation.Messages
				.Single(m => m.AuthorId == userId && !m.Sent.HasValue);
			
			Assert.NotNull(message);
			Assert.Equal("Sanitized", message.Contents);
		}

		/// <summary>
		/// Ensures that SendMessageAsync does not send a message with attachments
		/// that are too large.
		/// </summary>
		[Fact]
		public async Task SendMessageAsync_AttachmentErrors_DoesNotSendMessage()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate)
				.Build();
			
			var userId = GetUserId(database.Context, "Teacher1");
			var conversationId = database.Context.Conversations.Single().Id;
			database.Reload();

			var imageProcessor = GetMockImageProcessor();
			var messageService = CreateMessageService
			(
				database.Context, 
				imageProcessor: imageProcessor.Object
			);

			var sentResult = await messageService.SendMessageAsync
			(
				"Class1", 
				conversationId, 
				userId, 
				true /* admin */, 
				"Final",
				Collections.CreateList
				(
					new Attachment()
					{
						FileName = "attachment1.png",
						ContentType = "image/png",
						AttachmentData = new AttachmentData()
						{
							FileContents = new byte[5000000],
						}
					}	
				),
				conversationUrl: string.Empty,
				getAttachmentUrl: attachmentId => string.Empty,
				formatDateTime: dateTime => string.Empty
			);
			
			Assert.False(sentResult.result);
			Assert.Single(sentResult.attachmentErrors);
		}

		/// <summary>
		/// Ensures that SendMessageAsync sends the draft message when requested.
		/// </summary>
		[Fact]
		public async Task SendMessageAsync_NoAttachmentErrors_UpdatesAndSendsDraftMessage()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate)
				.Build();
			
			var userId = GetUserId(database.Context, "Teacher1");
			var conversation = database.Context.Conversations.Single();
			var conversationId = database.Context.Conversations.Single().Id;
			conversation.OwnerId = userId;
			conversation.Actionable = true;
			database.Context.Update(conversation);
			database.Context.SaveChanges();
			database.Reload();
			database.Reload();

			var attachment1 = new byte[5];
			var attachment2 = new byte[6];
			var downsampled = new byte[2];

			var timeProvider = GetMockTimeProvider(SentDate);
			var htmlSanitizer = GetMockHtmlSanitizer("Final", "Sanitized");
			var imageProcessor = GetMockImageProcessor(attachment1, downsampled);
			var emailProvider = GetMockEmailProvider
			(
				new [] { "Student1Email", "Teacher1Email" },
				"RE: [Class1 | First1 Last1]: Subject",
				"Sanitized"
			);

			var messageService = CreateMessageService
			(
				database.Context, 
				timeProvider: timeProvider.Object,
				htmlSanitizer: htmlSanitizer.Object,
				imageProcessor: imageProcessor.Object,
				emailProvider: emailProvider.Object
			);

			var sentResult = await messageService.SendMessageAsync
			(
				"Class1", 
				conversationId, 
				userId, 
				true /* admin */, 
				"Final",
				Collections.CreateList
				(
					new Attachment()
					{
						FileName = "attachment1.png",
						ContentType = "image/png",
						AttachmentData = new AttachmentData()
						{
							FileContents = attachment1,
						}
					},
					new Attachment()
					{
						FileName = "attachment2.png",
						ContentType = "image/png",
						AttachmentData = new AttachmentData()
						{
							FileContents = attachment2,
						}
					}	
				),
				conversationUrl: string.Empty,
				getAttachmentUrl: attachmentId => string.Empty,
				formatDateTime: dateTime => string.Empty
			);

			database.Reload();
			conversation = database.Context
				.Conversations
				.Include(c => c.Messages)
				.ThenInclude(m => m.Attachments)
				.ThenInclude(a => a.AttachmentData)
				.Single(c => c.Id == conversationId);
			var message = conversation.Messages
				.Single(m => m.AuthorId == userId && m.Contents == "Sanitized");
			
			Assert.True(sentResult.result);
			Assert.Null(sentResult.attachmentErrors);
			
			Assert.False(conversation.Actionable);
			Assert.Null(conversation.OwnerId);
			Assert.NotNull(message);
			Assert.Equal(SentDate, message.Sent);
			Assert.Equal(2, message.Attachments.Count);
			
			Assert.Equal("attachment1.jpg", message.Attachments[0].FileName);
			Assert.Equal("image/jpeg", message.Attachments[0].ContentType);
			Assert.Equal(2, message.Attachments[0].AttachmentData.FileContents.Length);
			
			Assert.Equal("attachment2.png", message.Attachments[1].FileName);
			Assert.Equal("image/png", message.Attachments[1].ContentType);
			Assert.Equal(6, message.Attachments[1].AttachmentData.FileContents.Length);
			
			emailProvider.VerifyAll();
		}
		
		/// <summary>
		/// Ensure that a student cannot remove a conversation that already has sent
		/// messages (i.e. is not a draft that the student created).
		/// </summary>
		[Fact]
		public async Task RemoveConversationAsync_Student_AlreadySentMessages_Throws()
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", "Student1", "Subject", SentDate)
				.Build();

			var studentId = GetUserId(database.Context, "Student1");
			var conversationId = database.Context.Conversations.Single().Id;
			database.Reload();

			var messageService = CreateMessageService(database.Context);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await messageService.RemoveConversationAsync
				(
					"Class1",
					conversationId,
					studentId,
					admin: false
				)
			);
		}
		
		/// <summary>
		/// Ensure that a student cannot remove a conversation that already has sent
		/// messages (i.e. is not a draft that the student created).
		/// </summary>
		[Theory]
		[InlineData("Student1", false /* admin */, true /* draft */)]
		[InlineData("Teacher1", true /* admin */, true /* draft */)]
		[InlineData("Teacher1", true /* admin */, false /* draft */)]
		public async Task RemoveConversationAsync_RemovesConversation( 
			string creator,
			bool admin,
			bool draft)
		{
			var database = GetDatabase()
				.AddConversation("Class1", "Student1", creator, "Subject", GetSentDate(draft))
				.Build();

			var userId = GetUserId(database.Context, creator);
			var conversationId = database.Context.Conversations.Single().Id;
			database.Reload();

			var messageService = CreateMessageService(database.Context);

			await messageService.RemoveConversationAsync
			(
				"Class1",
				conversationId,
				userId,
				admin
			);

			database.Reload();

			Assert.Null(database.Context.Conversations.SingleOrDefault());
		}

		/// <summary>
		/// Returns a populated database.
		/// </summary>
		private TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddSection("Class1", "Period2")
				.AddAdmin("Teacher1", "Last1", "First1", "Class1", superUser: false)
				.AddAdmin("Teacher2", "Last2", "First2", "Class1", superUser: false)
				.AddSectionRecipient("Teacher1", "Class1", "Period1")
				.AddStudent("Student1", "Last1", "First1", "Class1", "Period1")
				.AddStudent("Student2", "Last2", "First2", "Class1", "Period1")
				.AddStudent("Student3", "Last3", "First3", "Class1", "Period2");
		}

		/// <summary>
		/// Returns the ID of the given user.
		/// </summary>
		private int GetUserId(DatabaseContext dbContext, string userName)
		{
			return dbContext.Users.Single(u => u.UserName == userName).Id;
		}

		/// <summary>
		/// Returns a date if sent is true, and null otherwise.
		/// </summary>
		private DateTime? GetSentDate(bool draft)
		{
			return draft ? null : GetSentDate(0);
		}

		/// <summary>
		/// Returns a date, advanced by a given number of days.
		/// </summary>
		private DateTime? GetSentDate(int daysLater)
		{
			return SentDate + TimeSpan.FromDays(daysLater);
		}

		/// <summary>
		/// Creates a new time provider.
		/// </summary>
		private Mock<ITimeProvider> GetMockTimeProvider(DateTime dateTime)
		{
			var timeProvider = new Mock<ITimeProvider>();
			timeProvider.Setup(m => m.UtcNow).Returns(SentDate);
			return timeProvider;
		}

		/// <summary>
		/// Creates a new HTML sanitizer.
		/// </summary>
		private Mock<IHtmlSanitizer> GetMockHtmlSanitizer(string input, string output)
		{
			var htmlSanitizer = new Mock<IHtmlSanitizer>();
			htmlSanitizer.Setup(m => m.SanitizeHtml(input)).Returns(output);
			
			return htmlSanitizer;
		}

		/// <summary>
		/// Creates a new image processor.
		/// </summary>
		private Mock<IImageProcessor> GetMockImageProcessor(
			byte[] oldData = null, 
			byte[] newData = null)
		{
			var imageProcessor = new Mock<IImageProcessor>();
			imageProcessor
				.Setup
				(
					m => m.DownsampleImageToJpeg
					(
						oldData,
						It.IsAny<int>(),
						It.IsAny<int>(),
						out newData
					)
				).Returns(oldData != null);

			return imageProcessor;
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
							to => to.Select(r => r.EmailAddress)
								.ToHashSet()
								.SetEquals(expectedEmails)
						),
						subject,
						message
					)
				).Returns(Task.CompletedTask);
			
			return emailProvider;
		}
		
		/// <summary>
		/// Creates a new message service.
		/// </summary>
		private MessageService CreateMessageService(
			DatabaseContext dbContext,
			ITimeProvider timeProvider = null,
			IHtmlSanitizer htmlSanitizer = null,
			IImageProcessor imageProcessor = null,
			IEmailProvider emailProvider = null)
		{
			return new MessageService
			(
				dbContext, 
				new MockOperationRunner(), 
				timeProvider, 
				htmlSanitizer, 
				imageProcessor, 
				emailProvider
			);
		}
	}
}