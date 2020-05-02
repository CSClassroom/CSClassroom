using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
    public class DownloadSubmissionViewModel
    {
		[Display
		(
			Name = "Project",
			Description = "TODO: Project name"
		)]
		public string ProjectName { get; set; }

		[Display
		(
			Name = "Format",
			Description = "Select a download format."
		)]
		public string Format { get; set; }

		[Display
		(
			Name = "Sections",
			Description = "Select the sections to download."
		)]
		public string MultipleSections { get; set; }

		[Display
		(
			Name = "Section",
			Description = "TODO: section name"
		)]
		public string SingleSection { get; set; }
	}
}
