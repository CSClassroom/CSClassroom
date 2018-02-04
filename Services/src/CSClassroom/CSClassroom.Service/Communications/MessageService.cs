using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.Image;
using CSC.Common.Infrastructure.Security;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Model.Communications.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Communications
{
	/// <summary>
	/// The message service.
	/// </summary>
	public class MessageService : IMessageService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Retries operations.
		/// </summary>
		private readonly IOperationRunner _operationRunner;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// An HTML sanitizer.
		/// </summary>
		private readonly IHtmlSanitizer _htmlSanitizer;

		/// <summary>
		/// The image processor.
		/// </summary>
		private readonly IImageProcessor _imageProcessor;

		/// <summary>
		/// The e-mail provider.
		/// </summary>
		private readonly IEmailProvider _emailProvider;
		
		/// <summary>
		/// The number of attempts to make for retryable operations.
		/// </summary>
		private const int c_numAttempts = 5;

		/// <summary>
		/// The delay between attempts of retryable operations.
		/// </summary>
		private readonly TimeSpan c_delayBetweenAttempts
			= TimeSpan.FromSeconds(1);

		/// <summary>
		/// The maximum size for each attachment.
		/// </summary>
		private const int c_maxAttachmentSizeKb = 1024;

		/// <summary>
		/// The maximum image width.
		/// </summary>
		private const int c_maxImageWidth = 1280;
		
		/// <summary>
		/// The maximum image height.
		/// </summary>
		private const int c_maxImageHeight = 800;

		/// <summary>
		/// Constructor.
		/// </summary>
		public MessageService(
			DatabaseContext dbContext,
			IOperationRunner operationRunner,
			ITimeProvider timeProvider,
			IHtmlSanitizer htmlSanitizer,
			IImageProcessor imageProcessor,
			IEmailProvider emailProvider)
		{
			_dbContext = dbContext;
			_operationRunner = operationRunner;
			_timeProvider = timeProvider;
			_htmlSanitizer = htmlSanitizer;
			_imageProcessor = imageProcessor;
			_emailProvider = emailProvider;
		}

		/// <summary>
		/// Gets a list of conversations that are visible to the given user.
		/// Students see all conversations they are a part of, while admins see
		/// all conversations with students in the classes that they admin.
		/// </summary>
		public async Task<IOrderedQueryable<ConversationInfo>> GetConversationsAsync(
			string classroomName,
			int userId,
			int? studentId,
			bool admin)
		{
			var query = await GetConversationsQuery(classroomName, userId, admin);
			if (studentId.HasValue)
			{
				query = query.Where(c => c.StudentId == studentId);
			}
			
			return query
				.Select
				(
					c => new
					{
						c.Id,
						c.Student,
						c.Owner,
						c.Subject,
						ActionRequired = c.Actionable,
						LastReply = c.Messages
							.Where(m => m.Sent.HasValue)
							.OrderByDescending(m => m.Sent)
							.FirstOrDefault()
					}
				)
				.Select
				(
					c => new ConversationInfo
					{
						Id = c.Id,
						StudentLastName = c.Student.LastName,
						StudentFirstName = c.Student.FirstName,
						Subject = c.Subject,
						Actionable = c.ActionRequired,
						Owner = new OwnerInfo()
						{
							Id = c.Owner.Id,
							LastName = c.Owner.LastName,
							FirstName = c.Owner.FirstName
						},
						LastReply = new LastReplyInfo()
						{
							Sent = c.LastReply.Sent,
							LastName = c.LastReply.Author.LastName,
							FirstName = c.LastReply.Author.FirstName,
							PublicName = c.LastReply.Author.PublicName
						}
					}
				)
				.OrderByDescending(c => c.LastReply.Sent);
		}

		/// <summary>
		/// Returns the given conversation (or null if the user does not have permission).
		/// </summary>
		public async Task<Conversation> GetConversationAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool admin)
		{
			var query = await GetConversationsQuery(classroomName, userId, admin);

			return await query
				.Where(c => c.Id == conversationId)
				.Include(c => c.Classroom)
				.Include(c => c.Student)
				.Include(c => c.Owner)
				.Include(c => c.Messages)
				.ThenInclude(m => m.Author)
				.Include(c => c.Messages)
				.ThenInclude(m => m.Attachments)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns an attachment in a conversation.
		/// </summary>
		public async Task<Attachment> GetAttachmentAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool admin,
			int attachmentId)
		{
			var query = await GetConversationsQuery(classroomName, userId, admin);

			var conversation = await query
				.Where(c => c.Id == conversationId)
				.SingleOrDefaultAsync();

			if (conversation == null)
			{
				return null;
			}

			return await _dbContext.Attachments
				.Where(a => a.Message.ConversationId == conversationId)
				.Where(a => a.Id == attachmentId)
				.Include(a => a.AttachmentData)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns a list of students available to receive messages.
		/// </summary>
		public async Task<IList<ClassroomMembership>> GetStudentListAsync(
			string classroomName,
			int userId)
		{
			var sectionIds = await GetSectionIdsForAdminAsync(classroomName, userId);
			return await _dbContext.ClassroomMemberships
				.Include(cm => cm.User)
				.Include(cm => cm.SectionMemberships)
				.ThenInclude(sm => sm.Section)
				.Where
				(
					cm => cm.SectionMemberships.Any
					(
						sm => sectionIds.Contains(sm.SectionId)
					)
				)
				.OrderBy(cm => cm.User.LastName)
				.ThenBy(cm => cm.User.FirstName)
				.ToListAsync();
		}

		/// <summary>
		/// Creates a new conversation.
		/// </summary>
		public async Task<Conversation> CreateConversationAsync(
			string classroomName,
			int creatorId,
			int studentId,
			string subject)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			var conversation = new Conversation
			{
				ClassroomId = classroom.Id,
				CreatorId = creatorId,
				StudentId = studentId,
				Subject = subject,
				Messages = new List<Message>
				{
					new Message
					{
						AuthorId = creatorId
					}
				}
			};

			_dbContext.Conversations.Add(conversation);
			await _dbContext.SaveChangesAsync();

			return conversation;
		}

		/// <summary>
		/// Attempts to update the conversation owner. The owner is updated
		/// if the expected owner matches the actual owner at the time of the
		/// update. The up-to-date owner is returned, regardless of whether or
		/// not the update was successful.
		/// </summary>
		public async Task<User> UpdateConversationOwnerAsync(
			string classroomName,
			int conversationId,
			int userId,
			int? expectedOwnerId,
			int? newOwnerId)
		{
			return await RetryOperationIfNeededAsync(async () =>
			{
				var conversation = await GetConversationAsync
				(
					classroomName,
					conversationId,
					userId,
					admin: true
				);

				if (conversation.OwnerId != expectedOwnerId)
				{
					return conversation.Owner;
				}

				conversation.Owner = null;
				conversation.OwnerId = newOwnerId;
				_dbContext.Conversations.Update(conversation);

				await _dbContext.SaveChangesAsync();
				await _dbContext.Entry(conversation)
					.Reference(c => c.Owner)
					.LoadAsync();
				
				return conversation.Owner;
			});
		}

		/// <summary>
		/// Update the status of a conversation.
		/// </summary>
		public async Task UpdateConversationStatusAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool actionRequired)
		{
			await RetryOperationIfNeededAsync(async () =>
			{
				var conversation = await GetConversationAsync
				(
					classroomName,
					conversationId,
					userId,
					admin: true
				);

				conversation.Actionable = actionRequired;
				_dbContext.Conversations.Update(conversation);

				await _dbContext.SaveChangesAsync();
			});
		}
		
		/// <summary>
		/// Saves a draft of a message that has yet to be sent.
		/// </summary>
		public async Task SaveMessageDraftAsync(
			string classroomName,
			int conversationId,
			int authorId,
			bool admin,
			string contents)
		{
			await RetryOperationIfNeededAsync(async () =>
			{
				var conversation = await GetConversationAsync
				(
					classroomName,
					conversationId,
					authorId,
					admin
				);

				await UpdateDraftAsync(conversation, authorId, contents, send: false);
				
				await _dbContext.SaveChangesAsync();
			});
		}

		/// <summary>
		/// Sends a message.
		/// </summary>
		public async Task<(bool result, IList<string> attachmentErrors)> SendMessageAsync(
			string classroomName,
			int conversationId,
			int authorId,
			bool admin,
			string contents,
			IList<Attachment> attachments,
			string conversationUrl,
			Func<int, string> getAttachmentUrl,
			Func<DateTime, string> formatDateTime)
		{
			if (!ProcessAttachments(attachments, out var attachmentErrors))
			{
				return (result: false, attachmentErrors);
			}

			Message message = null;
			await RetryOperationIfNeededAsync(async () =>
			{
				var conversation = await GetConversationAsync
				(
					classroomName,
					conversationId,
					authorId,
					admin
				);

				message = await UpdateDraftAsync
				(
					conversation, 
					authorId, 
					contents, 
					true /*send*/, 
					attachments
				);

				conversation.Actionable = !admin;
				conversation.OwnerId = null;
				_dbContext.Update(conversation);
				
				await _dbContext.SaveChangesAsync();
			});

			await SendEmailAsync
			(
				message, 
				conversationUrl, 
				getAttachmentUrl, 
				formatDateTime
			);

			return (result: true, attachmentErrors: null);
		}

		/// <summary>
		/// Removes a conversation.
		/// </summary>
		public async Task RemoveConversationAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool admin)
		{
			await RetryOperationIfNeededAsync(async () =>
			{
				var query = await GetConversationsQuery(classroomName, userId, admin);
				var conversation = await query
					.Where(c => c.Id == conversationId)
					.Where(c => admin || !c.Messages.Any(m => m.Sent.HasValue))
					.SingleOrDefaultAsync();

				if (conversation == null)
				{
					throw new InvalidOperationException("Cannot remove conversation.");
				}

				_dbContext.Conversations.Remove(conversation);
				await _dbContext.SaveChangesAsync();
			});
		}

		/// <summary>
		/// Returns a query that returns all conversations accessible to a given user.
		/// </summary>
		private async Task<IQueryable<Conversation>> GetConversationsQuery(
			string classroomName,
			int userId,
			bool admin)
		{
			var conversationsQuery = _dbContext.Conversations
				.Where(c => c.Classroom.Name == classroomName)
				.Where(c => c.CreatorId == userId || c.Messages.Any(m => m.Sent.HasValue));

			if (admin)
			{
				var sectionIds = await GetSectionIdsForAdminAsync(classroomName, userId);
				conversationsQuery = conversationsQuery
					.Where
					(
						c => c.Student.ClassroomMemberships
							.Any
							(
								cm => cm.Classroom.Name == classroomName
								      && cm.SectionMemberships
									      .Any(sm => sectionIds.Contains(sm.SectionId))
							)
					);
			}
			else
			{
				conversationsQuery = conversationsQuery
					.Where(c => c.StudentId == userId);
			}

			return conversationsQuery;
		}

		/// <summary>
		/// Returns a list of section IDs for which the current user (an admin)
		/// is a section recipient.
		/// </summary>
		private async Task<IList<int>> GetSectionIdsForAdminAsync(
			string classroomName, 
			int userId)
		{
			return await _dbContext.SectionRecipients
				.Where(sr => sr.ClassroomMembership.UserId == userId)
				.Where(sr => sr.ClassroomMembership.Classroom.Name == classroomName)
				.Where(sr => sr.Section.AllowStudentMessages)
				.Where(sr => sr.ViewMessages)
				.Select(sr => sr.SectionId)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		private async Task<Classroom> LoadClassroomAsync(string classroomName)
		{
			return await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Updates the latest draft of a message for a given author.
		/// </summary>
		private async Task<Message> UpdateDraftAsync(
			Conversation conversation,
			int authorId, 
			string messageContents, 
			bool send,
			IList<Attachment> attachments = null)
		{
			var message = conversation.Messages
				.SingleOrDefault(m => m.AuthorId == authorId && !m.Sent.HasValue);

			bool existingMessage = message != null;
			if (message == null)
			{
				message = new Message
				{
					ConversationId = conversation.Id,
					AuthorId = authorId,
					Author = await _dbContext.Users
						.Where(u => u.Id == authorId)
						.SingleAsync()
				};
			}

			message.Contents = _htmlSanitizer.SanitizeHtml(messageContents);
			if (attachments != null)
			{
				message.Attachments = attachments;
			}
			
			if (send)
			{
				message.Sent = _timeProvider.UtcNow;
			}

			if (existingMessage)
			{
				_dbContext.Update(message);
			}
			else
			{
				_dbContext.Add(message);
			}

			return message;
		}

		/// <summary>
		/// Downsamples all image attachments, and ensures that each attachment
		/// is below the size limit.
		/// </summary>
		private bool ProcessAttachments(
			IList<Attachment> attachments, 
			out IList<string> attachmentErrors)
		{
			attachmentErrors = new List<string>();
			foreach (var attachment in attachments)
			{
				if (attachment.IsImage)
				{
					bool downsampled = _imageProcessor.DownsampleImageToJpeg
					(
						attachment.AttachmentData.FileContents,
						c_maxImageWidth,
						c_maxImageHeight,
						out var newImageData
					);

					if (downsampled)
					{
						attachment.AttachmentData.FileContents = newImageData;
						attachment.FileName = Path.ChangeExtension(attachment.FileName, "jpg");
						attachment.ContentType = "image/jpeg";
					}
				}

				if (attachment.AttachmentData.FileContents.Length 
				    > c_maxAttachmentSizeKb * 1024)
				{
					attachmentErrors.Add
					(
						$"Attachment {attachment.FileName} exceeds maximum size of "
							+ $"{c_maxAttachmentSizeKb} KB."
					);
				}
			}

			return !attachmentErrors.Any();
		}

		/// <summary>
		/// Sends an e-mail to the student and the admins of the class
		/// that are section recipients for the student's section.
		/// </summary>
		private async Task SendEmailAsync(
			Message message,
			string conversationUrl,
			Func<int, string> getAttachmentUrl,
			Func<DateTime, string> formatDateTime)
		{
			await _emailProvider.SendMessageAsync
			(
				await GetEmailRecipientsAsync(message),
				GetEmailSubject(message),
				GetEmailContents(message, conversationUrl, getAttachmentUrl, formatDateTime),
				GetEmailSender(message),
				GetEmailThreadInfo(message)
			);
		}

		/// <summary>
		/// Returns the e-mail recipients.
		/// </summary>
		private async Task<IList<EmailRecipient>> GetEmailRecipientsAsync(Message message)
		{
			var student = await _dbContext.Users
				.Where(user => user.Id == message.Conversation.StudentId)
				.Include(user => user.ClassroomMemberships)
				.ThenInclude(cm => cm.SectionMemberships)
				.SingleAsync();

			int sectionId = student.ClassroomMemberships
				.Single(cm => cm.ClassroomId == message.Conversation.ClassroomId)
				.SectionMemberships
				.First()
				.SectionId;

			var admins = await _dbContext.SectionRecipients
				.Where(sr => sr.SectionId == sectionId)
				.Where(sr => sr.EmailMessages)
				.Include(sr => sr.ClassroomMembership)
				.ThenInclude(cm => cm.User)
				.ToListAsync();

			var recipients = admins
				.Select(sr => sr.ClassroomMembership.User)
				.Union(new [] { student })
				.Select
				(
					user => new EmailRecipient
					(
						$"{user.FirstName} {user.LastName}",
						user.EmailAddress
					)
				)
				.ToList();

			return recipients;
		}

		/// <summary>
		/// Returns the e-mail subject.
		/// </summary>
		private string GetEmailSubject(Message message)
		{
			var student = message.Conversation.Student;
			var subject = $"[{message.Conversation.Classroom.Name} "
			              + $"| {student.FirstName} {student.LastName}]: "
			              + message.Conversation.Subject;

			if (message.Conversation.Messages.Count > 1)
			{
				subject = $"Re: {subject}";
			}

			return subject;
		}

		/// <summary>
		/// Returns the e-mail sender.
		/// </summary>
		private EmailSender GetEmailSender(Message message)
		{
			return new EmailSender
			(
				$"{message.Author.FirstName} {message.Author.LastName} (CS Classroom)",
				_emailProvider.DefaultFromAddress.Replace("@", $"-{message.Author.Id}@")
			);
		}

		/// <summary>
		/// Returns the e-mail contents.
		/// </summary>
		private string GetEmailContents(
			Message message,
			string conversationUrl,
			Func<int, string> getAttachmentUrl,
			Func<DateTime, string> formatDateTime)
		{
			var attachments = GetEmailAttachmentList(message, getAttachmentUrl);
			var footer = GetEmailFooter(message, formatDateTime, conversationUrl);
			
			return message.Contents + _htmlSanitizer.SanitizeHtml(attachments + footer);
		}

		/// <summary>
		/// Returns a list of attachments for an e-mail.
		/// </summary>
		private string GetEmailAttachmentList(
			Message message,
			Func<int, string> getAttachmentUrl)
		{
			if (message.Attachments == null || !message.Attachments.Any())
			{
				return string.Empty;
			}
			
			return 
				"<hr style=\"line-height: 5px; margin-bottom: 2px; margin-top: 10px\"/>"
				+ "<span style=\"font-weight: bold\">Attachments:</span>"
				+ "<br>"
				+ "<ul>"
				+ string.Join
				(
					"", 
					message.Attachments
						.OrderBy(a => a.FileName, new NaturalComparer())
						.Select
						(
							a => "<li>"
							     + $"<a href=\"{getAttachmentUrl(a.Id)}\">"
							     + $"{a.FileName}"
							     + "</a>"
							     + "</li>"
						)
				)
				+ "</ul>";
		}

		/// <summary>
		/// Returns the footer for an e-mail.
		/// </summary>
		private string GetEmailFooter(
			Message message,
			Func<DateTime, string> formatDateTime,
			string conversationUrl)
		{
			return
				"<hr style=\"line-height: 5px; margin-bottom: 2px; margin-top: 10px\"/>"
				+ "<span style=\"font-weight: bold\">"
				+ $"Sent by {message.Author.PubliclyDisplayedName} "
				+ $"on {formatDateTime(message.Sent.Value)}</span>"
				+ "<br><br>"
				+ "Do not reply to this e-mail. Instead, click "
				+ $"<a href=\"{conversationUrl}\">here</a> "
				+ "to reply.";
		}

		/// <summary>
		/// Returns the e-mail threading information.
		/// </summary>
		private ThreadInfo GetEmailThreadInfo(Message message)
		{
			var otherMessages = message.Conversation.Messages
				.Where(m => m != message)
				.Where(m => m.Sent.HasValue)
				.OrderBy(m => m.Sent)
				.ToList();
			
			return new ThreadInfo
			(
				GetMessageId(message),
				otherMessages.Any() 
					? GetMessageId(otherMessages[otherMessages.Count - 1]) 
					: null,
				otherMessages.Any() 
					? otherMessages.Select(GetMessageId).ToList()
					: null
			);
		}

		/// <summary>
		/// Returns the message ID for a message sent at the given time.
		/// </summary>
		private string GetMessageId(Message message)
		{
			var domain = _emailProvider.DefaultFromAddress
				.Substring(_emailProvider.DefaultFromAddress.IndexOf("@") + 1);

			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var ms = Convert.ToInt64((message.Sent.Value - epoch).TotalMilliseconds);
		
			return $"{ms}.{message.AuthorId}@{domain}";
		}

		/// <summary>
		/// Executes an operation, retrying the operation if an optimistic
		/// concurrency exception is encountered.
		/// </summary>
		private Task RetryOperationIfNeededAsync(
			Func<Task> operation)
		{
			return _operationRunner.RetryOperationIfNeededAsync
			(
				operation,
				ex => ex is DbUpdateConcurrencyException,
				c_numAttempts,
				c_delayBetweenAttempts,
				defaultResultIfFailed: false
			);
		}

		/// <summary>
		/// Executes an operation, retrying the operation if an optimistic
		/// concurrency exception is encountered.
		/// </summary>
		private Task<TResult> RetryOperationIfNeededAsync<TResult>(
			Func<Task<TResult>> operation)
		{
			return _operationRunner.RetryOperationIfNeededAsync
			(
				operation,
				ex => ex is DbUpdateConcurrencyException,
				c_numAttempts,
				c_delayBetweenAttempts,
				defaultResultIfFailed: false
			);
		}
	}
}

