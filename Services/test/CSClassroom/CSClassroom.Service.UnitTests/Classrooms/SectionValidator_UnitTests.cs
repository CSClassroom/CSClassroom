using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Classrooms
{
	/// <summary>
	/// Unit tests for the section validator.
	/// </summary>
	public class SectionValidator_UnitTests
	{
		/// <summary>
		/// Ensures that an error occurs when validating a section containing more than
		/// one section gradebook with the same classroom gradebook.
		/// </summary>
		[Fact]
		public async Task ValidateSectionAsync_DuplicateSectionGradebooks_Error()
		{
			var database = GetDatabase().Build();
			var classroomGradebookId = database.Context
				.ClassroomGradebooks
				.First()
				.Id;
			
			var errors = new MockErrorCollection();
			var validator = new SectionValidator(database.Context);

			var section = new Section()
			{
				SectionGradebooks = Collections.CreateList
				(
					new SectionGradebook()
					{
						ClassroomGradebookId = classroomGradebookId,
					},
					new SectionGradebook()
					{
						ClassroomGradebookId = classroomGradebookId,
					}
				)
			};

			var result = await validator.ValidateSectionAsync(section, errors);
			
			Assert.False(result);
			Assert.True(errors.VerifyErrors("SectionGradebooks"));
		}
		
		/// <summary>
		/// Ensures that an error occurs when validating a section containing duplicate
		/// section recipients.
		/// </summary>
		[Fact]
		public async Task ValidateSectionAsync_DuplicateSectionRecipients_Error()
		{
			var database = GetDatabase().Build();
			var adminId = database.Context
				.ClassroomMemberships
				.Single(cm => cm.User.UserName == "Admin1")
				.Id;
			
			var errors = new MockErrorCollection();
			var validator = new SectionValidator(database.Context);

			var section = new Section()
			{
				SectionRecipients = Collections.CreateList
				(
					new SectionRecipient()
					{
						ClassroomMembershipId = adminId,
					},
					new SectionRecipient()
					{
						ClassroomMembershipId = adminId,
					}
				)
			};

			var result = await validator.ValidateSectionAsync(section, errors);
			
			Assert.False(result);
			Assert.True(errors.VerifyErrors("SectionRecipients"));
		}
		
		/// <summary>
		/// Ensures that an error occurs when validating a section containing duplicate
		/// section recipients.
		/// </summary>
		[Fact]
		public async Task ValidateSectionAsync_NonAdminSectionRecipients_Error()
		{
			var database = GetDatabase().Build();
			var studentId = database.Context
				.ClassroomMemberships
				.Single(cm => cm.User.UserName == "Student1")
				.Id;
			
			var errors = new MockErrorCollection();
			var validator = new SectionValidator(database.Context);

			var section = new Section()
			{
				SectionRecipients = Collections.CreateList
				(
					new SectionRecipient()
					{
						ClassroomMembershipId = studentId,
					}
				)
			};

			var result = await validator.ValidateSectionAsync(section, errors);
			
			Assert.False(result);
			Assert.True(errors.VerifyErrors("SectionRecipients"));
		}
		
		/// <summary>
		/// Ensures that a valid section is successfully validated.
		/// </summary>
		[Fact]
		public async Task ValidateSectionAsync_ValidSection_NoErrors()
		{
			var database = GetDatabase().Build();
			var classroomGradebookId = database.Context
				.ClassroomGradebooks
				.First()
				.Id;
			var adminId = database.Context
				.ClassroomMemberships
				.Single(cm => cm.User.UserName == "Student1")
				.Id;
			
			var errors = new MockErrorCollection();
			var validator = new SectionValidator(database.Context);

			var section = new Section()
			{
				SectionGradebooks = Collections.CreateList
				(
					new SectionGradebook()
					{
						ClassroomGradebookId = classroomGradebookId
					}	
				),
				
				SectionRecipients = Collections.CreateList
				(
					new SectionRecipient()
					{
						ClassroomMembershipId = adminId,
					}
				)
			};

			var result = await validator.ValidateSectionAsync(section, errors);
			
			Assert.False(result);
			Assert.True(errors.VerifyErrors("SectionRecipients"));
		}

		/// <summary>
		/// Returns a populated database.
		/// </summary>
		private TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddClassroomGradebook("Class1", "Gradebook1")
				.AddAdmin("Admin1", "Last1", "First1", "Class1", superUser: false)
				.AddStudent("Student1", "Last1", "First1", "Class1", "Period1");
		}
	}
}