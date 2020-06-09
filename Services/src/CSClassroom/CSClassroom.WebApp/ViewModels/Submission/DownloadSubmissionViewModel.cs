using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using CSC.CSClassroom.WebApp.ViewModels.Shared;
using CSC.Common.Infrastructure.Projects.Submissions;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// The master view model for the download submission page, used for downloading
	/// Eclipse projects and flat file lists from student project repos.  The download submissions
	/// page is a form which dynamically decides which controls to show, and which to hide.
	/// Initially, the "main download form" controls are visible, which give the top-level
	/// options.  When the user clicks one of the "select students" link, the form is redrawn
	/// with the main download options hidden, and a checklist of students from the specified
	/// section shown.
	/// </summary>
	public class DownloadSubmissionViewModel
	{
		/// <summary>
		/// If -1, render the main Download form controls; else, render the "select students" form
		/// controls for the section represented by SectionsAndStudents[IndexForSectionStudentsView]
		/// </summary>
		public int IndexForSectionStudentsView {get; set; }

		[Display
		(
			Name = "Components to download",
			Description = "Select which components of the student submissions to include in the download."
		)]
		public ProjectSubmissionDownloadFormat Format { get; set; }

		[Display
		(
			Name = "Include unsubmitted code",
			Description = "Check this to include the latest commit from students who did not turn in their code.  Uncheck this to skip downloading those students' code."
		)]
		public bool IncludeUnsubmitted { get; set; }

		[Display
		(
			Name = "Sections",
			Description = "Select the sections to download."
		)]
		public List<SectionsAndStudents> SectionsAndStudents { get; set; }

		/// <summary>
		///  The section originally active from the submissions view when the user
		///  clicked download.  This informs the default options to be selected
		/// </summary>
		public SectionInfo CurrentSection { get; set; }

		/// <summary>
		/// The submit button to finally initiate the download
		/// </summary>
		public string DownloadSubmitButton { get; set; }

		public IEnumerable<SelectListItem> GetDownloadFormatSelectList()
		{
			return new SelectListItem[]
			{
				new SelectListItem("Flat file list", ProjectSubmissionDownloadFormat.Flat.ToString(), false),
				new SelectListItem("Eclipse projects", ProjectSubmissionDownloadFormat.Eclipse.ToString(), false),
				new SelectListItem("All", ProjectSubmissionDownloadFormat.All.ToString(), true),
			};
		}
	}

	/// <summary>
	/// Info on the "current section", which is what the user had clicked on before
	/// choosing to Download.  This is used to populate the default section option
	/// from the Download page.
	/// </summary>
	public class SectionInfo
	{
		public string Name { get; set; }
		public int Index { get; set; }
	}

    /// <summary>
    ///  Info for a single student to download
    /// </summary>
    public class StudentToDownload
	{
		[Display
		(
			Name = "IsSelected",
			Description = "Download?"
		)]
		public Boolean Selected { get; set; }

		/// <summary>
		/// The unique ID for the user.
		/// </summary>
		public int Id { get; set; }

		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Display(Name = "Submitted?")]
		public Boolean Submitted { get; set; }
	}

	/// <summary>
	/// Info on each section to download, including the students within
	/// that section to download.
	/// </summary>
	public class SectionsAndStudents
	{
		/// <summary>
		///  Name of section selected for download
		/// </summary>
		public SelectListItem SectionName { get; set; }

		public int SectionId { get; set; }

        /// <summary>
        ///  Students selected by user to download
        /// </summary>
        [Display
		(
			Name = "Students",
			Description = "Select the students to download."
		)]
		public List<StudentToDownload> SelectedStudents { get; set; }

		/// <summary>
		///  The submit button (rendered in link style) to display the form controls to select a student
		/// </summary>
		public string SectionsAndStudentsSubmitButton { get; set; }
	}
}
