namespace CSC.CSClassroom.Model.Communications
{
	/// <summary>
	/// An attachment to a message.
	/// </summary>
	public class Attachment
	{
		/// <summary>
		/// The primary key for the attachment.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The message that the file is attached to.
		/// </summary>
		public Message Message { get; set; }
		public int MessageId { get; set; }
		
		/// <summary>
		/// The name of the file.
		/// </summary>
		public string FileName { get; set; }
		
		/// <summary>
		/// The content type of the file.
		/// </summary>
		public string ContentType { get; set; }
		
		/// <summary>
		/// The contents of the attachment.
		/// </summary>
		public AttachmentData AttachmentData { get; set; }
		
		/// <summary>
		/// Returns whether or not the content type corresponds to an image file.
		/// </summary>
		public bool IsImage =>
			ContentType.ToLower() == "image/jpg" ||
			ContentType.ToLower() == "image/jpeg" ||
			ContentType.ToLower() == "image/pjpeg" ||
			ContentType.ToLower() == "image/gif" ||
			ContentType.ToLower() == "image/x-png" ||
			ContentType.ToLower() == "image/png";
	}
}