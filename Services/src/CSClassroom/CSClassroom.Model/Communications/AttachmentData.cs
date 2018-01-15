namespace CSC.CSClassroom.Model.Communications
{
	/// <summary>
	/// The actual file data for an attachment.
	/// </summary>
	public class AttachmentData
	{
		/// <summary>
		/// The primary key for the attachment data.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The attachment containing this data.
		/// </summary>
		public Attachment Attachment { get; set; }
		public int AttachmentId { get; set; }
		
		/// <summary>
		/// The contents of the file.
		/// </summary>
		public byte[] FileContents { get; set; }
	}
}