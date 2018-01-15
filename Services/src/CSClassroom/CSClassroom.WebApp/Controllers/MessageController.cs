using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using CSC.CSClassroom.WebApp.Settings;
using CSC.CSClassroom.WebApp.ViewModels.Assignment;
using CSC.CSClassroom.WebApp.ViewModels.Message;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReflectionIT.Mvc.Paging;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The message controller.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	public class MessageController : BaseClassroomController
	{
		/// <summary>
		/// The message service.
		/// </summary>
		private IMessageService MessageService { get; }

		/// <summary>
		/// The domain name of the service.
		/// </summary>
		private readonly WebAppHost _webAppHost;

		/// <summary>
		/// Constructor.
		/// </summary>
		public MessageController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			IMessageService messageService,
			WebAppHost webAppHost)
				: base(args, classroomService)
		{
			MessageService = messageService;
			_webAppHost = webAppHost;
		}

		/// <summary>
		/// Returns whether or not the resource exists.
		/// </summary>
		protected override bool DoesResourceExist()
		{
			var anySections = ClassroomMembership.SectionMemberships
				?.Any(sm => sm.Section.AllowStudentMessages) 
			    ?? false;
			
			return base.DoesResourceExist() && (anySections || IsAdmin);
		}

		/// <summary>
		/// Lists all conversations for the current user.
		/// </summary>
		[Route("Messages")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Index(
			int page = 1, 
			int pageSize = 10, 
			int? studentId = null)
		{
			await PopulateDropDownListsAsync();
			
			var conversationsQuery = await MessageService.GetConversationsAsync
			(
				ClassroomName, 
				User.Id,
				studentId,
				IsAdmin
			);

			var viewModel = new IndexViewModel
			(
				await PagingList.CreateAsync
				(
					conversationsQuery,
					pageSize: pageSize,
					pageIndex: page
				),
				pageSize,
				studentId
			);

			return View(viewModel);
		}

		/// <summary>
		/// Creates a new conversation.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("NewMessage")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Create(string subject, int studentId)
		{
			var conversation = await MessageService.CreateConversationAsync
			(
				ClassroomName,
				User.Id,
				IsAdmin ? studentId : User.Id,
				subject
			);
			
			return RedirectToAction("Show", new { conversationId = conversation.Id });
		}

		/// <summary>
		/// Views an existing conversation.
		/// </summary>
		[Route("Messages/{conversationId}")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Show(int conversationId)
		{
			var conversation = await MessageService.GetConversationAsync
			(
				ClassroomName,
				conversationId,
				User.Id,
				IsAdmin
			);

			if (conversation == null)
			{
				return NotFound();
			}

			return View(conversation);
		}

		/// <summary>
		/// Returns an attachment in a conversation.
		/// </summary>
		[Route("Messages/{conversationId}/Attachments/{attachmentId}")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> GetAttachment(
			int conversationId,
			int attachmentId)
		{
			var attachment = await MessageService.GetAttachmentAsync
			(
				ClassroomName,
				conversationId,
				User.Id,
				IsAdmin,
				attachmentId
			);

			if (attachment == null)
			{
				return NotFound();
			}

			return File
			(
				attachment.AttachmentData.FileContents, 
				attachment.ContentType, 
				attachment.FileName
			);
		}

		/// <summary>
		/// Attempts to update the conversation owner. The owner is updated
		/// if the expected owner matches the actual owner at the time of the
		/// update. The up-to-date owner is returned, regardless of whether or
		/// not the update was successful.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Messages/{conversationId}/UpdateOwner")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> UpdateOwner(
			int conversationId,
			int? expectedOwnerId,
			int? newOwnerId)
		{
			var user = await MessageService.UpdateConversationOwnerAsync
			(
				ClassroomName,
				conversationId,
				User.Id,
				expectedOwnerId,
				newOwnerId
			);

			return Json
			(
				new
				{
					newOwnerId = user?.Id,
					newOwnerName = user != null 
						? $"{user.FirstName} {user.LastName}" 
						: "None"
				}
			);
		}

		/// <summary>
		/// Updates the status of a conversation.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Messages/{conversationId}/UpdateStatus")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> UpdateStatus(
			int conversationId,
			bool actionRequired)
		{
			await MessageService.UpdateConversationStatusAsync
			(
				ClassroomName,
				conversationId,
				User.Id,
				actionRequired
			);

			return Ok();
		}

		/// <summary>
		/// Updates a draft reply in a conversation.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Messages/{conversationId}/SaveDraftReply")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> SaveDraftReply(
			int conversationId, 
			string messageContents)
		{
			await MessageService.SaveMessageDraftAsync
			(
				ClassroomName,
				conversationId,
				User.Id,
				IsAdmin,
				messageContents
			);

			return Ok();
		}

		/// <summary>
		/// Sends a reply in a conversation.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Messages/{conversationId}/SendReply")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> SendReply(
			int conversationId, 
			string messageContents,
			IEnumerable<IFormFile> attachments)
		{
			var sendResult = await MessageService.SendMessageAsync
			(
				ClassroomName,
				conversationId,
				User.Id,
				IsAdmin,
				messageContents,
				await GetAttachmentsAsync(attachments),
				_webAppHost.HostName + Url.Action("Show", new { conversationId }),
				attachmentId => 
					_webAppHost.HostName 
					+ Url.Action("GetAttachment", new { conversationId, attachmentId }),
				dateTime => dateTime.FormatLongDateTime(TimeZoneProvider)
			);

			return Json(new
			{
				result = sendResult.result,
				attachmentErrors = sendResult.attachmentErrors
			});
		}

		/// <summary>
		/// Deletes a conversation.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Messages/{conversationId}/Delete")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Delete(int conversationId)
		{
			await MessageService.RemoveConversationAsync
			(
				ClassroomName, 
				conversationId,
				User.Id,
				IsAdmin
			);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Populates all drop-down menus.
		/// </summary>
		private async Task PopulateDropDownListsAsync()
		{
			PopulatePageSizeDropDown();

			if (IsAdmin)
			{
				await PopulateStudentDropDownAsync();
			}
		}

		/// <summary>
		/// Populates a list of potential page sizes.
		/// </summary>
		private void PopulatePageSizeDropDown()
		{
			ViewBag.PageSizes = new int[] {10, 20, 50, 100}
				.Select(size => size.ToString())
				.Select(size => new SelectListItem() { Text = size, Value = size})
				.ToList();
		}

		/// <summary>
		/// Populates the drop-down menu containing a list of students.
		/// </summary>
		private async Task PopulateStudentDropDownAsync()
		{
			var students = await MessageService.GetStudentListAsync
			(
				ClassroomName,
				User.Id
			);

			var sections = students
				.GroupBy(cm => cm.SectionMemberships[0].Section)
				.ToDictionary
				(
					s => s.Key.Id,
					s => new SelectListGroup {Name = s.Key.DisplayName}
				);

			var studentItems = students
				.Select
				(
					student => new SelectListItem()
					{
						Text = $"{student.User.LastName}, {student.User.FirstName}",
						Value = student.UserId.ToString(),
						Group = sections[student.SectionMemberships[0].SectionId]
					}
				)
				.OrderBy(item => item.Group?.Name)
				.ToList();

			ViewBag.StudentFilter = new List<SelectListItem>()
				{
					new SelectListItem()
					{
						Text = "All Students",
						Selected = true,
						Value = "All"
					},
				}
				.Union(studentItems)
				.ToList();

			ViewBag.NewMessageStudents = new List<SelectListItem>()
				{
					new SelectListItem()
					{
						Text = "Select a student...",
						Value = "",
						Disabled = true,
						Selected = true
					},
				}
				.Union(studentItems)
				.ToList();
		}

		/// <summary>
		/// Converts form files to attachment objects.
		/// </summary>
		private async Task<IList<Attachment>> GetAttachmentsAsync(
			IEnumerable<IFormFile> formFiles)
		{
			if (formFiles == null)
			{
				return new List<Attachment>();
			}

			return await Task.WhenAll
			(
				formFiles.Select
				(
					async formFile => new Attachment()
					{
						ContentType = formFile.ContentType,
						FileName = WebUtility.HtmlEncode
						(
							Path.GetFileName(formFile.FileName)
						),
						AttachmentData = new AttachmentData()
						{
							FileContents = await GetFileContentsAsync(formFile)
						}
					}
				)
			);
		}

		/// <summary>
		/// Returns the contents of a form file. 
		/// </summary>
		private async Task<byte[]> GetFileContentsAsync(IFormFile formFile)
		{
			using (var stream = new MemoryStream())
			{
				await formFile.CopyToAsync(stream);
				return stream.ToArray();
			}	
		}
	}
}
