namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// The type of access to a section.
	/// </summary>
	public enum SectionRole
	{
		/// <summary>
		/// No access to anything.
		/// </summary>
		None = 0,
		
		/// <summary>
		/// Can view information about their own assignments and projects.
		/// </summary>
		Student = 100,

		/// <summary>
		/// Can view information about all student assignments and projects,
		/// and grade projects.
		/// </summary>
		Staff = 200,

		/// <summary>
		/// Can perform all operations in a given classroom.
		/// </summary>
		Admin = 300
	}
}