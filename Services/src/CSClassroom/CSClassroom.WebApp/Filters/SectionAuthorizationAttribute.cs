using System;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Controllers;

namespace CSC.CSClassroom.WebApp.Filters
{
	/// <summary>
	/// A filter that rejects access for anonymous users if sign-in
	/// is required.
	/// </summary>
	public class SectionAuthorizationAttribute : ClassroomAuthorizationAttribute
	{
		/// <summary>
		/// The required role.
		/// </summary>
		public SectionRole SectionRoleRequired { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionAuthorizationAttribute(SectionRole sectionRoleRequired)
			: base(ClassroomRole.General)
		{
			SectionRoleRequired = sectionRoleRequired;
		}

		/// <summary>
		/// Returns whether or not the user is authorized.
		/// </summary>
		public override bool IsAuthorized(BaseController baseController)
		{
			if (!base.IsAuthorized(baseController))
				return false;

			var controller = baseController as BaseSectionController;
			if (controller == null)
			{
				throw new InvalidOperationException(
					"Controller must inherit from BaseSectionController.");
			}

			return (controller.SectionRole >= SectionRoleRequired);
		}
	}
}
