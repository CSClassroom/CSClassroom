using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Communications
{
	/// <summary>
	/// A conversation consists of one or more messages between a student
	/// and class admins.
	/// </summary>
	public class Conversation
	{
		/// <summary>
		/// The ID of the conversation.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The classroom this message is relevant to.
		/// </summary>
		public Classroom Classroom { get; set; }
		public int ClassroomId { get; set; }
		
		/// <summary>
		/// The creator of the conversation.
		/// </summary>
		public User Creator { get; set; }
		public int CreatorId { get; set; }
		
		/// <summary>
		/// The student in the conversation.
		/// </summary>
		public User Student { get; set; }
		public int StudentId { get; set; }
		
		/// <summary>
		/// The current owner of the conversation. The owner is the class admin who
		/// is currently writing a reply.
		/// </summary>
		public User Owner { get; set; }
		[ConcurrencyCheck]
		public int? OwnerId { get; set; }
		
		/// <summary>
		/// The subject of the conversation.
		/// </summary>
		public string Subject { get; set; }
		
		/// <summary>
		/// The messages in the conversation.
		/// </summary>
		public IList<Message> Messages { get; set; }
		
		/// <summary>
		/// Whether or not an admin should reply to this message.
		/// </summary>
		public bool Actionable { get; set; }
	}
}