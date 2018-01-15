using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Ensures that a section is valid.
	/// </summary>
	public interface ISectionValidator
	{
		/// <summary>
		/// Returns whether or not a section is valid.
		/// </summary>
		Task<bool> ValidateSectionAsync(
			Section section, 
			IModelErrorCollection errors);
	}
}