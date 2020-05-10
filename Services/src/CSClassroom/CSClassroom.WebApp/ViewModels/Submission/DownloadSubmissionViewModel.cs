using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// The category of submission components to download
	/// </summary>
	public enum DownloadFormat
	{
		[Display(Name = "Flat file list")]
		Flat,

		[Display(Name = "Eclipse projects")]
		Eclipse,

		[Display(Name = "All")]
		All,
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
		public string StudentName { get; set; }
		public int StudentID { get; set; }
	}

	/// <summary>
	/// Info on each section to download, including the students within
	/// that section to download.
	/// </summary>
	public class SectionStudentsToDownload
	{
		/// <summary>
		///  Name of section selected for download
		/// </summary>
		public SelectListItem SectionName { get; set; }

		/// <summary>
		/// If true, download ALL students from this section, and ignore SelectedStudents
		/// </summary>
		public Boolean AllStudents { get; set; }

		/// <summary>
		///  Students selected by user to download
		/// </summary>
		public List<StudentToDownload> SelectedStudents { get; set; }
	}

	/// <summary>
	/// The master view model for the download submission page, used for downloading
	/// Eclipse projects and flat file lists from student project repos
	/// </summary>
	public class DownloadSubmissionViewModel
    {
		[Display
		(
			Name = "Components to download",
			Description = "Select which components of the student submissions to include in the download."
		)]
		public DownloadFormat Format { get; set; }

		[Display
		(
			Name = "Sections",
			Description = "Select the sections to download."
		)]
		public List<SectionStudentsToDownload> SectionStudents { get; set; }

        public SectionInfo CurrentSection { get; set; }
	}
}
