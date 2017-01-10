using System.Collections.Generic;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The view model for a build.
	/// </summary>
	public class HistoryViewModel
	{
		/// <summary>
		/// The user who committed the code.
		/// </summary>
		public User User { get; }

		/// <summary>
		/// The user's builds.
		/// </summary>
		public IList<Model.Projects.Build> Builds { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public HistoryViewModel(User user, IList<Model.Projects.Build> builds)
		{
			User = user;
			Builds = builds;
		}
	}
}
