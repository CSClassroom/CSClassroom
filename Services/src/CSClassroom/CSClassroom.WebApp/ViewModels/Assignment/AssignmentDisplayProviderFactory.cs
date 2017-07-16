using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Produces an AssignmentDisplayProvider for a given assignment result.
	/// </summary>
	public class AssignmentDisplayProviderFactory : IAssignmentDisplayProviderFactory
	{
		/// <summary>
		/// The timezone provider.
		/// </summary>
		private readonly ITimeZoneProvider _timeZoneProvider;

		/// <summary>
		/// The assignment URL provider.
		/// </summary>
		private readonly IAssignmentUrlProvider _assignmentUrlProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentDisplayProviderFactory(
			ITimeZoneProvider timeZoneProvider,
			IAssignmentUrlProvider assignmentUrlProvider)
		{
			_timeZoneProvider = timeZoneProvider;
			_assignmentUrlProvider = assignmentUrlProvider;
		}

		/// <summary>
		/// Creates an AssignmentDisplayProvider.
		/// </summary>
		public IAssignmentDisplayProvider CreateDisplayProvider(
			AssignmentResult assignmentResult)
		{
			if (assignmentResult.CombinedSubmissions)
			{
				return new CombinedSubmissionsDisplayProvider
				(
					_timeZoneProvider,
					_assignmentUrlProvider,
					(CombinedSubmissionsAssignmentResult) assignmentResult
				);
			}
			else
			{
				return new SeparateSubmissionsDisplayProvider
				(
					_timeZoneProvider,
					_assignmentUrlProvider,
					(SeparateSubmissionsAssignmentResult)assignmentResult
				);
			}
		}
	}
}
