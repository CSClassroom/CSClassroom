using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
    public class DownloadSubmissionViewModel
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
		public List<SelectListItem> SectionNames { get; set; }
	}
}
