namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// The type of access to a classroom.
	/// </summary>
	public enum ClassroomRole
	{
		/// <summary>
		/// No access to anything.
		/// </summary>
		None = 0,

		/// <summary>
		/// Can read public resources in a given classroom, and solve problems.
		/// </summary>
		General = 100,

		/// <summary>
		/// Can perform all operations in a given classroom.
		/// </summary>
		Admin = 200
	}
}
