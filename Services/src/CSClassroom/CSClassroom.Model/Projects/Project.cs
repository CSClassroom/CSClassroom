using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A project.
	/// </summary>
	public class Project
	{
		/// <summary>
		/// The primary key.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the classroom that this project belongs to.
		/// </summary>
		public int ClassroomId { get; set; }

		/// <summary>
		/// The classroom.
		/// </summary>
		public Classroom Classroom { get; set; }

		/// <summary>
		/// The name of the project.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = ModelStrings.OnlyAlphanumeric)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the project that will appear in URLs and "
				+ "GitHub repositories. This must be unique for the class."
		)]
		public string Name { get; set; }

		/// <summary>
		/// Whether or not project commits should be built.
		/// </summary>
		[Display
		(
			Name = "Build Commits",
			Description = "Select whether student commits for this project will be built and tested."
		)]
		public bool BuildCommits { get; set; }

		/// <summary>
		/// Whether or not explicit submissions are required.
		/// </summary>
		[Display
		(
			Name = "Submissions",
			Description = "Select whether this project will require students to submit " +
				"for each checkpoint."
		)]
		public bool ExplicitSubmissionRequired { get; set; }

		/// <summary>
		/// Paths to private files that are not distributed to student 
		/// project repositories, but are used for testing.
		/// </summary>
		[Display
		(
			Name = "Private File Paths",
			Description = "Enter a list of file paths (or wildcard expressions) "
				+ "for files that should not be copied to student repositories. "
				+ "The paths must be relative to the repository root."
		)]
		public virtual List<PrivateFilePath> PrivateFilePaths { get; set; }

		/// <summary>
		/// Paths to immutable files that are always taken from the project template,
		/// for purposes of testing. Any student changes to such files are ignored.
		/// </summary>
		[Display
		(
			Name = "Immutable File Paths",
			Description = "Enter a list of file paths (or wildcard expressions) "
				+ "for files that should not be changed by students in their "
				+ "project repositories. Any student changes made to these files "
				+ "are ignored during building and testing."
		)]
		public virtual List<ImmutableFilePath> ImmutableFilePaths { get; set; }

		/// <summary>
		/// The test classes whose tests will be run on project submissions.
		/// </summary>
		[Display
		(
			Name = "Test Classes",
			Description = "Enter a list of fully qualified test class names to run "
				+ "when testing student commits. "
		)]
		public virtual List<TestClass> TestClasses { get; set; }

		/// <summary>
		/// Checkpoints for the project.
		/// </summary>
		public virtual List<Checkpoint> Checkpoints { get; set; }

		/// <summary>
		/// Returns the repository name for the project template.
		/// </summary>
		public string TemplateRepoName => $"{Name}_Template";
		
		/// <summary>
		/// Returns the name of the student project repository.
		/// </summary>
		public string GetStudentRepoName(ClassroomMembership student)
		{
			return $"{Name}_{student.GitHubTeam}";
		}
	}
}
