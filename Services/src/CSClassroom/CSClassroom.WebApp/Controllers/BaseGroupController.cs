using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSC.CSClassroom.Model.Classrooms;
using Microsoft.AspNetCore.Mvc.Filters;
using CSC.CSClassroom.Service.Classrooms;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The base class for controllers managing resources in a group.
	/// </summary>
	public class BaseGroupController : BaseController
	{
		/// <summary>
		/// The group service.
		/// </summary>
		protected IGroupService GroupService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseGroupController(IGroupService groupService)
		{
			GroupService = groupService;
		}

		/// <summary>
		/// The group name route string.
		/// </summary>
		private const string c_groupNameRouteStr = "groupName";

		/// <summary>
		/// The group route prefix.
		/// </summary>
		protected const string GroupRoutePrefix = "Groups/{" + c_groupNameRouteStr + "}";

		/// <summary>
		/// The name of the current group.
		/// </summary>
		protected string GroupName => (string)RouteData.Values[c_groupNameRouteStr];

		/// <summary>
		/// The current group.
		/// </summary>
		protected Group Group { get; private set; }

		/// <summary>
		/// Ensures the current group is valid.
		/// </summary>
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			Group = await GroupService.GetGroupAsync(GroupName);
			if (Group == null)
			{
				context.Result = new NotFoundResult();
			}

			await base.OnActionExecutionAsync(context, next);
		}
	}
}
