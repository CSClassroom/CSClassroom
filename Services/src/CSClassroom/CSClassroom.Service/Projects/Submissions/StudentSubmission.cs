using System;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// The contents of a student submission.
	/// </summary>
	public class StudentSubmission : IDisposable
	{
		/// <summary>
		/// The student.
		/// </summary>
		public ClassroomMembership Student { get; }

		/// <summary>
		/// The contents of the student submission.
		/// </summary>
		public IArchive Contents { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentSubmission(
			ClassroomMembership student, 
			IArchive contents)
		{
			Student = student;
			Contents = contents;
		}

		/// <summary>
		/// Disposes of this object.
		/// </summary>
		public void Dispose()
		{
			Contents.Dispose();
		}
	}
}
