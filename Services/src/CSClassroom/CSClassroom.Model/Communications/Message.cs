using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Communications
{
	/// <summary>
	/// A single message in a conversation.
	/// </summary>
	public class Message
	{
		/// <summary>
		/// The ID of the message.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The conversation that the message is a part of.
		/// </summary>
		public Conversation Conversation { get; set; }
		public int ConversationId { get; set; }
		
		/// <summary>
		/// The author of the message.
		/// </summary>
		public User Author { get; set; }
		public int AuthorId { get; set; }
		
		/// <summary>
		/// The contents of the message.
		/// </summary>
		public string Contents { get; set; }
		
		/// <summary>
		/// When the message was sent.
		/// </summary>
		public DateTime? Sent { get; set; }
		
		/// <summary>
		/// The attachments for this message, if any.
		/// </summary>
		public IList<Attachment> Attachments { get; set; }
	}
}