using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.Validators
{
	/// <summary>
	/// Validates add/update operations for questions.
	/// </summary>
	public interface IQuestionValidator
	{
		/// <summary>
		/// Ensures that a question to add or update is in a valid state.
		/// </summary>
		Task<bool> ValidateQuestionAsync(
			Question question,
			IModelErrorCollection errors,
			string classroomName);
	}
}
