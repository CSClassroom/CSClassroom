using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Identity
{
	using Identity = Model.Users.Identity;

	/// <summary>
	/// An identity provider.
	/// </summary>
	public interface IIdentityProvider
	{
		/// <summary>
		/// The identity of the currently logged-in, potentially unregistered user.
		/// </summary>
		Identity CurrentIdentity { get; }
	}
}
