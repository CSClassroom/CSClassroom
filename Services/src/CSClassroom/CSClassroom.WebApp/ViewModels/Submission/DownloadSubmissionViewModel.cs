using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// The master view model for the download submission page, used for downloading
	/// Eclipse projects and flat file lists from student project repos
	/// </summary>
	public class DownloadSubmissionViewModel
	{
		/// <summary>
		/// TODO: Determines type of form to render
		/// </summary>
		public int IndexForSectionStudentsView {get; set; }

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
		public List<SectionsAndStudents> SectionsAndStudents { get; set; }

		/// <summary>
		///  The section originally active from the submissions view when the user
		///  clicked download.  This informs the default options to be selected
		/// </summary>
		public SectionInfo CurrentSection { get; set; }
	}

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
	public class SectionsAndStudents
	{
		private const int c_maxStudentsToDisplayInSelectionLink = 3;

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

		/// <summary>
		/// Text to use in the link to edit the list of students to download for this section
		/// </summary>
        public string StudentDisplayList
        {
            get
            {
				if (AllStudents)
				{
					return "Students: All";
				}

				int numStudents = SelectedStudents == null ? 0 : SelectedStudents.Count;
				if (numStudents == 0)
				{
					// TODO: This should be caught earlier and should cause the checkbox to be unchecked
					// and this link to be disabled / hidden
					return "Students: None";
				}

				// TODO: Does this do last name first?
				string ret = "Students: " + SelectedStudents[0].StudentName;
				for (int i=1; i < Math.Min(c_maxStudentsToDisplayInSelectionLink, numStudents); i++)
				{
					ret += ", " + SelectedStudents[i].StudentName;
				}

				int remaining = numStudents - c_maxStudentsToDisplayInSelectionLink;
				if (remaining > 0)
				{
					ret += ", and " + remaining + " more";
				}

				return ret;
			}
        }

		public string SubmitButton { get; set; }

    }
}
