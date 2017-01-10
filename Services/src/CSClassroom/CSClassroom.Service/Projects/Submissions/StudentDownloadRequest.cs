using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// A request to download a student repository.
	/// </summary>
	public class StudentDownloadRequest
	{
		/// <summary>
		/// The student.
		/// </summary>
		public ClassroomMembership Student { get; }

		/// <summary>
		/// Whether or not the student has submitted.
		/// </summary>
		public bool Submitted { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentDownloadRequest(
			ClassroomMembership student,
			bool submitted)
		{
			Student = student;
			Submitted = submitted;
		}
	}
}
