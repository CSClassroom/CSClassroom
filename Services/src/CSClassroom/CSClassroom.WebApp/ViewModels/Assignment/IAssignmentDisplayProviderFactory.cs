using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Produces an AssignmentDisplayProvider for a given assignment result.
	/// </summary>
	public interface IAssignmentDisplayProviderFactory
	{
		/// <summary>
		/// Creates an AssignmentDisplayProvider.
		/// </summary>
		IAssignmentDisplayProvider CreateDisplayProvider(
			AssignmentResult assignmentResult);
	}
}
