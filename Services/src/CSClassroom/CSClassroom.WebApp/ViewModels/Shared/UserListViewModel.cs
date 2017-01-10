using System.Collections.Generic;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// A list of users to display.
	/// </summary>
	public class UserListViewModel
	{
		/// <summary>
		/// The users to display.
		/// </summary>
		public IList<User> Users { get; }

		/// <summary>
		/// The actions to show.
		/// </summary>
		public IList<UserAction> Actions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserListViewModel(IList<User> users, IList<UserAction> actions)
		{
			Users = users;
			Actions = actions;
		}
	}
}
