using System;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Communications.ServiceResults
{
	/// <summary>
	/// Information about the last reply to a conversation.
	/// </summary>
	public class LastReplyInfo
	{
		/// <summary>
		/// The time the last reply was sent. Must be nullable to
		/// satisfy EF.
		/// </summary>
		public DateTime? Sent { get; set; }
		
		/// <summary>
		/// The last name of the last replier (if any).
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// The first name of the last replier (if any).
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// The public name of the last replier (if any).
		/// </summary>
		public string PublicName { get; set; }
		
		/// <summary>
		/// Returns the user's publicly displayed name if present,
		/// or the user's first and last name if not.
		/// </summary>
		public string PubliclyDisplayedName => 
			PublicName ?? $"{FirstName} {LastName}";
	}

	/// <summary>
	/// Information about the owner of a conversation.
	/// </summary>
	public class OwnerInfo
	{
		/// <summary>
		/// The current owner ID. Must be nullable to satisfy EF.
		/// </summary>
		public int? Id { get; set; }
		
		/// <summary>
		/// The last name of the owner, if any.
		/// </summary>
		public string LastName { get; set; }
		
		/// <summary>
		/// The first name of the owner, if any.
		/// </summary>
		public string FirstName { get; set; }
	}
	
	/// <summary>
	/// Information about a conversation.
	/// </summary>
	public class ConversationInfo
	{
		/// <summary>
		/// The ID of the conversation.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The last name of the student in the conversation.
		/// </summary>
		public string StudentLastName { get; set; }
		
		/// <summary>
		/// The first name of the student in the conversation.
		/// </summary>
		public string StudentFirstName { get; set; }
		
		/// <summary>
		/// The subject of the conversation.
		/// </summary>
		public string Subject { get; set; }
		
		/// <summary>
		/// Whether or not action is required by an admin.
		/// </summary>
		public bool Actionable { get; set; }
		
		/// <summary>
		/// Returns whether or not the given user can take action
		/// on this message.
		/// </summary>
		public bool ActionableByUser(int userId) 
			=> Actionable && (Owner == null || Owner.Id == userId);

		/// <summary>
		/// The last reply of the conversation.
		/// </summary>
		private LastReplyInfo _lastReply;
		public LastReplyInfo LastReply
		{
			get => _lastReply;
			set 
			{
				if (value.Sent != null)
				{
					_lastReply = value;
				}
			}
		}

		/// <summary>
		/// The owner of the conversation.
		/// </summary>
		private OwnerInfo _owner;
		public OwnerInfo Owner
		{
			get => _owner;
			set 
			{
				if (value.Id != null)
				{
					_owner = value;
				}
			}
		}
	}
}