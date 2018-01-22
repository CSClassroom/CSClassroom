using System;
using System.Threading.Tasks;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Controllers;
using CSC.CSClassroom.WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSC.CSClassroom.WebApp.Filters
{
	/// <summary>
	/// A filter that checks the existence of a resource before executing
	/// the corresponding action.
	/// </summary>
	public class CheckExistenceAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
	{
		/// <summary>
		/// Run after all other filters.
		/// </summary>
		public int Order => 1;
		
		/// <summary>
		/// Ensures a resource exists before the corresponding action is executed.
		/// </summary>
		public async Task OnActionExecutionAsync(
			ActionExecutingContext context, 
			ActionExecutionDelegate next)
		{
			var baseController = context.Controller as BaseController;
			if (baseController == null)
			{
				throw new InvalidOperationException(
					"Controller must inherit from BaseController.");
			}

			if (!baseController.DoesResourceExist())
			{
				context.Result = baseController.NotFound();
				return;
			}

			await next();
		}
	}
}
