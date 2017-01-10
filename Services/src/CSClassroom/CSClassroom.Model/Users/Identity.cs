namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// An identity.
	/// </summary>
	public class Identity
	{
		/// <summary>
		/// A unique ID for the user.
		/// </summary>
		public string UniqueId { get; }

		/// <summary>
		/// The username for the user.
		/// </summary>
		public string UserName { get; }

		/// <summary>
		/// The first name of the user.
		/// </summary>
		public string FirstName { get; }

		/// <summary>
		/// The last name of the user.
		/// </summary>
		public string LastName { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public Identity(
			string uniqueId, 
			string userName, 
			string firstName, 
			string lastName)
		{
			UniqueId = uniqueId;
			UserName = userName;
			FirstName = firstName;
			LastName = lastName;
		}
	}
}
