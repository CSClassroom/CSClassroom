using System;
using System.Threading.Tasks;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Controllers;
using CSC.CSClassroom.WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSC.CSClassroom.WebApp.Filters
{
	/// <summary>
	/// The type of user required to be authorized.
	/// </summary>
	public enum RequiredAccess
	{
		Anonymous = 0,
		Authenticated = 100,
		Registered = 200,
		SuperUser = 300
	}

	/// <summary>
	/// A filter that rejects access for anonymous users if sign-in
	/// is required.
	/// </summary>
	public class AuthorizationAttribute : Attribute, IAsyncActionFilter
	{
		/// <summary>
		/// The access required for the resource.
		/// </summary>
		public RequiredAccess AccessRequired { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AuthorizationAttribute(RequiredAccess accessRequired)
		{
			AccessRequired = accessRequired;
		}

		/// <summary>
		/// Ensures the user is signed in if being signed in is required.
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
			
			if (AccessRequired >= RequiredAccess.Authenticated 
				&& baseController.IdentityState == IdentityState.Anonymous)
			{
				context.Result = baseController.RedirectToSignIn();
				return;
			}

			if (AccessRequired >= RequiredAccess.Registered 
				&& baseController.IdentityState == IdentityState.Unregistered)
			{
				context.Result = baseController.Forbid();
				return;
			}
			
			if (AccessRequired >= RequiredAccess.SuperUser 
				&& baseController.IdentityState != IdentityState.SuperUser)
			{
				context.Result = baseController.Forbid();
				return;
			}

			if (baseController.IdentityState != IdentityState.SuperUser 
				&& !IsAuthorized(baseController))
			{
				context.Result = baseController.Forbid();
				return;
			}

			await next();
		}

		/// <summary>
		/// Returns whether or not the user is authorized.
		/// </summary>
		public virtual bool IsAuthorized(BaseController baseController)
		{
			// Subclasses should override this method for custom authorization.

			return true;
		}
	}
}
