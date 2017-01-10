using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Identity;
using Microsoft.AspNetCore.Http;

namespace CSC.CSClassroom.WebApp.Providers
{
	/// <summary>
	/// Provides the current user.
	/// </summary>
	public class IdentityProvider : IIdentityProvider
	{
		/// <summary>
		/// The HTTP context accessor.
		/// </summary>
		private IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// The current identity.
		/// </summary>
		public Identity CurrentIdentity => GetIdentity();

		/// <summary>
		/// The claim type for the OID claim.
		/// </summary>
		private const string c_oidClaimType 
			= "http://schemas.microsoft.com/identity/claims/objectidentifier";

		/// <summary>
		/// Constructor.
		/// </summary>
		public IdentityProvider(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// Returns the current identity, or null if none.
		/// </summary>
		private Identity GetIdentity()
		{
			var claimsPrincipal = _httpContextAccessor.HttpContext.User;
			if (claimsPrincipal.Identity?.Name != null)
			{
				var claims = claimsPrincipal.Claims;

				return new Identity
				(
					GetClaimValue(claims, c_oidClaimType),
					claimsPrincipal.Identity.Name,
					GetClaimValue(claims, ClaimTypes.GivenName),
					GetClaimValue(claims, ClaimTypes.Surname)
				);
			}

			return null;
		}

		/// <summary>
		/// Returns the value of a claim, or null if there is no such claim.
		/// </summary>
		private string GetClaimValue(IEnumerable<Claim> claims, string claimType)
		{
			return claims
				.FirstOrDefault(claim => claim.Type == claimType)
				?.Value;
		}
	}
}
