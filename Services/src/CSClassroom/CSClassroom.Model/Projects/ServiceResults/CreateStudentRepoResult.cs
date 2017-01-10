using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The result of attempting to create a repository with the required data.
	/// </summary>
	public enum CreateAndPushResult
	{
		Exists,
		Created,
		Overwritten,
		Failed
	}

	/// <summary>
	/// The result of creating a student project repository.
	/// </summary>
	public class CreateStudentRepoResult
	{
		/// <summary>
		/// The student.
		/// </summary>
		public User Student { get; private set; }

		/// <summary>
		/// The result for creating the repository.
		/// </summary>
		public CreateAndPushResult CreateAndPushResult { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CreateStudentRepoResult(User student, CreateAndPushResult createAndPushResult)
		{
			Student = student;
			CreateAndPushResult = createAndPushResult;
		}
	}
}
