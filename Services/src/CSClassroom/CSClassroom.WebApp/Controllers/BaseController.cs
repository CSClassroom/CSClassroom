using System;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.Logging;
using CSC.CSClassroom.WebApp.ModelBinders;
using CSC.CSClassroom.WebApp.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// Arguments for the base controller.
	/// </summary>
	public class BaseControllerArgs
	{
		/// <summary>
		/// The logger.
		/// </summary>
		public ILogContext LogContext { get; }

		/// <summary>
		/// The identity provider.
		/// </summary>
		public IIdentityProvider IdentityProvider { get; }

		/// <summary>
		/// The user provider.
		/// </summary>
		public IUserProvider UserProvider { get; }

		/// <summary>
		/// The time zone provider.
		/// </summary>
		public ITimeZoneProvider TimeZoneProvider { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseControllerArgs(
			ILogContext logContext,
			IIdentityProvider identityProvider,
			IUserProvider userProvider,
			ITimeZoneProvider timeZoneProvider)
		{
			LogContext = logContext;
			IdentityProvider = identityProvider;
			UserProvider = userProvider;
			TimeZoneProvider = timeZoneProvider;
		}
	}

	/// <summary>
	/// The base class for all controllers in the web app.
	/// </summary>
	public class BaseController : Controller
	{
		/// <summary>
		/// The current identity state.
		/// </summary>
		public IdentityState IdentityState { get; private set; }

		/// <summary>
		/// The current user (or null if none).
		/// </summary>
		public new User User { get; private set; }

		/// <summary>
		/// The time zone provider.
		/// </summary>
		protected ITimeZoneProvider TimeZoneProvider { get; }

		/// <summary>
		/// The logger.
		/// </summary>
		protected ILogContext LogContext { get; }

		/// <summary>
		/// The identity provider.
		/// </summary>
		private readonly IIdentityProvider _identityProvider;

		/// <summary>
		/// The user provider.
		/// </summary>
		private readonly IUserProvider _userProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseController(BaseControllerArgs args)
		{
			LogContext = args.LogContext;
			TimeZoneProvider = args.TimeZoneProvider;
			_identityProvider = args.IdentityProvider;
			_userProvider = args.UserProvider;
		}

		/// <summary>
		/// Runs before the action is executing.
		/// </summary>
		public override async Task OnActionExecutionAsync(
			ActionExecutingContext context,
			ActionExecutionDelegate next)
		{
			if (!context.Filters.OfType<AuthorizationAttribute>().Any())
			{
				throw new InvalidOperationException(
					"All controllers/actions must have an authorization filter.");
			}

			if (!await _userProvider.IsServiceActivatedAsync())
			{
				context.Result = RedirectToAction("ActivateService", "Activation");
				return;
			}

			await InitializeAsync();

			if (!DoesResourceExist())
			{
				context.Result = NotFound();
				return;
			}

			await next();
		}

		/// <summary>
		/// Executes before the action is executed.
		/// </summary>
		protected virtual async Task InitializeAsync()
		{
			IdentityState = await _userProvider.GetCurrentIdentityStateAsync();
			User = await _userProvider.GetCurrentUserAsync();

			ViewBag.User = User;
			ViewBag.ActionName = (string)ControllerContext.RouteData.Values["action"];
		}

		/// <summary>
		/// Returns whether or not the resource exists.
		/// </summary>
		protected virtual bool DoesResourceExist()
		{
			// Subclasses should override this method if they are managing
			// a resource that might not exist.

			return true;
		}

		/// <summary>
		/// A wrapper around the model state, for passing to services that need to
		/// populate the collection with errors.
		/// </summary>
		protected IModelErrorCollection ModelErrors => new ModelErrorCollection(ModelState);
	}
}
