using System;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Controllers;

namespace CSC.CSClassroom.WebApp.Filters
{
	/// <summary>
	/// A filter that rejects access for anonymous users if sign-in
	/// is required.
	/// </summary>
	public class ClassroomAuthorizationAttribute : AuthorizationAttribute
	{
		/// <summary>
		/// The required role 
		/// </summary>
		public ClassroomRole ClassroomRoleRequired { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassroomAuthorizationAttribute(ClassroomRole classroomRoleRequired)
			: base(RequiredAccess.Registered)
		{
			ClassroomRoleRequired = classroomRoleRequired;
		}

		/// <summary>
		/// Returns whether or not the user is authorized.
		/// </summary>
		public override bool IsAuthorized(BaseController baseController)
		{
			if (!base.IsAuthorized(baseController))
				return false;
			
			var controller = baseController as BaseClassroomController;
			if (controller == null)
			{
				throw new InvalidOperationException(
					"Controller must inherit from BaseClassroomController.");
			}

			return (controller.ClassroomRole >= ClassroomRoleRequired);
		}
	}
}
