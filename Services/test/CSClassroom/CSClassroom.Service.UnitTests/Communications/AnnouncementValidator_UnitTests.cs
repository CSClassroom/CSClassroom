using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Service.Communications;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Communications
{
	/// <summary>
	/// Unit tests for the AnnouncementValidator class.
	/// </summary>
	public class AnnouncementValidator_UnitTests
	{

		/// <summary>
		/// Ensures that ValidateAnnouncement errors out if no sections are included.
		/// </summary>
		[Fact]
		public void ValidateAnnouncement_NoSections_ReturnsFalseWithError()
		{
			var validator = new AnnouncementValidator();
			var modelErrors = new MockErrorCollection();
			var result = validator.ValidateAnnouncement
			(
				new Classroom(),
				new Announcement()
				{
					Sections = new List<AnnouncementSection>()
				},
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Sections"));
		}

		/// <summary>
		/// Ensures that ValidateAnnouncement errors out if the same section
		/// is included twice.
		/// </summary>
		[Fact]
		public void ValidateAnnouncement_DuplicateSections_ReturnsFalseWithError()
		{
			var validator = new AnnouncementValidator();
			var modelErrors = new MockErrorCollection();
			var result = validator.ValidateAnnouncement
			(
				new Classroom(),
				new Announcement()
				{
					Sections = Collections.CreateList
					(
						new AnnouncementSection() { SectionId = 1 },
						new AnnouncementSection() { SectionId = 1 }
					)
				},
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Sections"));
		}

		/// <summary>
		/// Ensures that ValidateAnnouncement errors out if a section ID is included
		/// that is not a valid section of the given class.
		/// </summary>
		[Fact]
		public void ValidateAnnouncement_InvalidSection_ReturnsFalseWithError()
		{
			var validator = new AnnouncementValidator();
			var modelErrors = new MockErrorCollection();
			var result = validator.ValidateAnnouncement
			(
				new Classroom()
				{
					Sections = new List<Section>()
				},
				new Announcement()
				{
					Sections = Collections.CreateList
					(
						new AnnouncementSection() { SectionId = 1 }
					)
				},
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Sections"));
		}

		/// <summary>
		/// Ensures that ValidateAnnouncement succeeds if the announcement
		/// includes valid sections.
		/// </summary>
		[Fact]
		public void ValidateAnnouncement_ValidSections_ReturnsTrueWithNoError()
		{
			var validator = new AnnouncementValidator();
			var modelErrors = new MockErrorCollection();
			var result = validator.ValidateAnnouncement
			(
				new Classroom()
				{
					Sections = Collections.CreateList
					(
						new Section() { Id = 1}
					)
				},
				new Announcement()
				{
					Sections = Collections.CreateList
					(
						new AnnouncementSection() { SectionId = 1 }
					)
				},
				modelErrors
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);
		}
	}
}
