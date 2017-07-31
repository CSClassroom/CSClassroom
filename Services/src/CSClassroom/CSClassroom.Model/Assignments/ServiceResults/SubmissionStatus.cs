using System;
using System.Collections.Generic;
using System.Linq;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// The completion of a submission.
	/// </summary>
	public enum Completion
	{
		Completed,
		InProgress,
		NotStarted
	}

	/// <summary>
	/// The status of a submission.
	/// </summary>
	public class SubmissionStatus
	{
		/// <summary> 
		/// The completion of the submission.
		/// </summary>
		public Completion Completion { get; }

		/// <summary>
		/// Whether or not the submission was late.
		/// </summary>
		public bool Late { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionStatus(Completion completion, bool late)
		{
			Completion = completion;
			Late = late;
		}
	}
}
