namespace CSC.CSClassroom.Model.Users
{
	/// <summary>
	/// The activation token required to create a new super user. 
	/// </summary>
	public class ActivationToken
	{
		/// <summary>
		/// The value of the token.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ActivationToken(string value)
		{
			Value = value;
		}
	}
}
