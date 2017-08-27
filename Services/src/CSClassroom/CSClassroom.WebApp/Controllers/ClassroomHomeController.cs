using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Communications;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.ViewModels.Assignment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReflectionIT.Mvc.Paging;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The announcement controller.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	public class ClassroomHomeController : BaseClassroomController
	{
		/// <summary>
		/// The classroom service.
		/// </summary>
		private IAnnouncementService AnnouncementService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassroomHomeController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			IAnnouncementService announcementService)
				: base(args, classroomService)
		{
			AnnouncementService = announcementService;
		}

		/// <summary>
		/// Shows the home page for the class.
		/// </summary>
		[Route("")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Index(int page = 1)
		{
			var announcementsQuery = await AnnouncementService.GetAnnouncementsAsync
			(
				ClassroomName, 
				User.Id,
				ClassroomRole >= ClassroomRole.Admin
			);

			var announcements = await PagingList<Announcement>.CreateAsync
			(
				announcementsQuery,
				pageSize: 5,
				pageIndex: page
			);

			return View(announcements);
		}

		/// <summary>
		/// Creates a new assignment.
		/// </summary>
		[Route("PostAnnouncement")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> PostAnnouncement()
		{
			ViewBag.OperationType = "Post";
			return View("PostEditAnnouncement");
		}

		/// <summary>
		/// Creates a new assignment.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("PostAnnouncement")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> PostAnnouncement(Announcement announcement)
		{
			ModelState[nameof(Announcement.DatePosted)].ValidationState 
                = ModelValidationState.Valid;

			if (ModelState.IsValid
			    && await AnnouncementService.PostAnnouncementAsync
					(
						ClassroomName, 
						User.Id, 
						announcement,
						FormatDateTime,
						ModelErrors
					))
			{
				return RedirectToAction("Index");
			}
			else
			{
				ViewBag.OperationType = "Post";
				return View("PostEditAnnouncement", announcement);
			}
		}

		/// <summary>
		/// Edits an announcement.
		/// </summary>
		[Route("EditAnnouncement")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> EditAnnouncement(int announcementId)
		{
			var announcement = await AnnouncementService.GetAnnouncementAsync
			(
				ClassroomName,
				announcementId
			);

			if (announcement == null)
			{
				return NotFound();
			}

			ViewBag.OperationType = "Edit";
			return View("PostEditAnnouncement", announcement);
		}

		/// <summary>
		/// Edits an announcement.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("EditAnnouncement")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> EditAnnouncement(Announcement announcement)
		{
			ModelState[nameof(Announcement.DatePosted)].ValidationState
				= ModelValidationState.Valid;

			if (ModelState.IsValid
			    && await AnnouncementService.EditAnnouncementAsync
			    (
				    ClassroomName,
				    announcement,
					FormatDateTime,
				    ModelErrors
			    ))
			{
				return RedirectToAction("Index");
			}
			else
			{
				ViewBag.OperationType = "Edit";
				return View("PostEditAnnouncement", announcement);
			}
		}


		/// <summary>
		/// Deletes an announcement.
		/// </summary>
		[Route("Announcements/{announcementId}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DeleteAnnouncement(int announcementId)
		{
			var announcement = await AnnouncementService.GetAnnouncementAsync
			(
				ClassroomName, 
				announcementId
			);

			if (announcement == null)
			{
				return NotFound();
			}

			return View(announcement);
		}

		/// <summary>
		/// Deletes an announcement.
		/// </summary>
		[HttpPost, ActionName("DeleteAnnouncement")]
		[ValidateAntiForgeryToken]
		[Route("Announcements/{announcementId}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DeleteAnnouncementConfirmed(int announcementId)
		{
			await AnnouncementService.DeleteAnnouncementAsync(ClassroomName, announcementId);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Formats the given datetime object.
		/// </summary>
		private string FormatDateTime(DateTime dateTime)
		{
			return dateTime.FormatLongDateTime(TimeZoneProvider);
		}
	}
}
