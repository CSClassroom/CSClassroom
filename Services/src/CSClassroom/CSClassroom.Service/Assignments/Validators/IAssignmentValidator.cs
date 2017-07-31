using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.Validators
{
	/// <summary>
	/// Ensures that a new or existing assignment is valid.
	/// </summary>
	public interface IAssignmentValidator
	{
		/// <summary>
		/// Validates that an assignment is correctly configured.
		/// </summary>
		Task<bool> ValidateAssignmentAsync(
			Assignment assignment,
			IModelErrorCollection modelErrors);
	}
}
