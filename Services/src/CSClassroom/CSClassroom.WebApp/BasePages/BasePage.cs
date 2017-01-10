using CSC.CSClassroom.Model.Users;
using Microsoft.AspNetCore.Mvc.Razor;

namespace CSC.CSClassroom.WebApp.BasePages
{
	/// <summary>
	/// The base page for a view using in a particular classroom.
	/// </summary>
	public abstract class BasePage<TModel> : RazorPage<TModel>
	{
		/// <summary>
		/// The current classroom.
		/// </summary>
		public new User User => ViewBag.User;

		/// <summary>
		/// The action name.
		/// </summary>
		public string ActionName => ViewBag.ActionName;
	}
}
