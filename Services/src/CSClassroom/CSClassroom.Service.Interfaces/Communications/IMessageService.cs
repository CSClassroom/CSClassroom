using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Model.Communications.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Communications
{
	/// <summary>
	/// The message service.
	/// </summary>
	public interface IMessageService
	{
		/// <summary>
		/// Gets a list of conversations that are visible to the given user.
		/// Students see all conversations they are a part of, while admins see
		/// all conversations with students in the classes that they admin.
		/// </summary>
		Task<IOrderedQueryable<ConversationInfo>> GetConversationsAsync(
			string classroomName, 
			int userId,
			int? studentId,
			bool admin);

		/// <summary>
		/// Returns the given conversation.
		/// </summary>
		Task<Conversation> GetConversationAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool admin);

		/// <summary>
		/// Returns an attachment for a message in a conversation.
		/// </summary>
		Task<Attachment> GetAttachmentAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool admin,
			int attachmentId);

		/// <summary>
		/// Returns a list of students available to receive messages.
		/// </summary>
		Task<IList<ClassroomMembership>> GetStudentListAsync(
			string classroomName,
			int userId);

		/// <summary>
		/// Creates a new conversation.
		/// </summary>
		Task<Conversation> CreateConversationAsync(
			string classroomName,
			int creatorId,
			int studentId,
			string subject);

		/// <summary>
		/// Attempts to update the conversation owner. The owner is updated
		/// if the expected owner matches the actual owner at the time of the
		/// update. The up-to-date owner is returned, regardless of whether or
		/// not the update was successful.
		/// </summary>
		Task<User> UpdateConversationOwnerAsync(
			string classroomName,
			int conversationId,
			int userId,
			int? expectedOwnerId,
			int? newOwnerId);

		/// <summary>
		/// Update the status of a conversation.
		/// </summary>
		Task UpdateConversationStatusAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool actionRequired);

		/// <summary>
		/// Saves a draft of a message that has yet to be sent.
		/// </summary>
		Task SaveMessageDraftAsync(
			string classroomName,
			int conversationId,
			int authorId,
			bool admin,
			string contents);

		/// <summary>
		/// Sends a message.
		/// </summary>
		Task<(bool result, IList<string> attachmentErrors)> SendMessageAsync(
			string classroomName,
			int conversationId,
			int authorId,
			bool admin,
			string contents,
			IList<Attachment> attachments,
			string conversationUrl,
			Func<int, string> getAttachmentUrl,
			Func<DateTime, string> formatDateTime);

		/// <summary>
		/// Removes a conversation.
		/// </summary>
		Task RemoveConversationAsync(
			string classroomName,
			int conversationId,
			int userId,
			bool admin);
	}
}
