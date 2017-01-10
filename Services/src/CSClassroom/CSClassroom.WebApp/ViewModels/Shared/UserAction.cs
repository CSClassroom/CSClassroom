using System;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// An action that can be taken for a given user.
	/// </summary>
	public class UserAction
	{
		/// <summary>
		/// The name of the action.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Returns the URL for the given action, on the given user.
		/// </summary>
		public Func<User, string> GetActionUrl { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserAction(string actionName, Func<User, string> getActionUrl)
		{
			Name = actionName;
			GetActionUrl = getActionUrl;
		}
	}
}
