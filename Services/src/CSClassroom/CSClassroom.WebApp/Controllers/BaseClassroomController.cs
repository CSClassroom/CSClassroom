using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The base class for controllers managing resources in a classroom.
	/// </summary>
	public class BaseClassroomController : BaseController
	{
		/// <summary>
		/// The classroom service.
		/// </summary>
		protected IClassroomService ClassroomService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseClassroomController(
			BaseControllerArgs args,
			IClassroomService classroomService)
				: base(args)
		{
			ClassroomService = classroomService;
		}

		/// <summary>
		/// The classroom name route string.
		/// </summary>
		private const string c_classroomNameRouteStr = "className";

		/// <summary>
		/// The classroom route prefix.
		/// </summary>
		protected const string ClassroomRoutePrefix = "Classes/{" + c_classroomNameRouteStr + "}";

		/// <summary>
		/// The name of the current classroom.
		/// </summary>
		protected string ClassroomName => (string)RouteData.Values[c_classroomNameRouteStr];

		/// <summary>
		/// The current classroom.
		/// </summary>
		public Classroom Classroom { get; private set; }

		/// <summary>
		/// The current classroom membership.
		/// </summary>
		public ClassroomMembership ClassroomMembership { get; private set; }

		/// <summary>
		/// The current classroom membership (if any).
		/// </summary>
		public ClassroomRole ClassroomRole { get; private set; }

		/// <summary>
		/// The classrooms the current user has access to.
		/// </summary>
		public IList<ClassroomMembership> ClassroomsWithAccess { get; private set; }

		/// <summary>
		/// Executes before the action is executed.
		/// </summary>
		protected override async Task InitializeAsync()
		{
			await base.InitializeAsync();

			Classroom = await ClassroomService.GetClassroomAsync(ClassroomName);
			ClassroomMembership = GetClassroomMembership(User, Classroom);
			ClassroomRole = GetClassroomRole(User, Classroom);
			ClassroomsWithAccess = User != null
				? await ClassroomService.GetClassroomsWithAccessAsync(User.Id)
				: new List<ClassroomMembership>();

			ViewBag.Classroom = Classroom;
			ViewBag.ClassroomRole = ClassroomRole;
			ViewBag.ClassroomWithAccess = ClassroomsWithAccess;
		}

		/// <summary>
		/// Returns whether or not the resource exists.
		/// </summary>
		public override bool DoesResourceExist()
		{
			return Classroom != null;
		}

		/// <summary>
		/// Returns whether or not the user is a classroom admin.
		/// </summary>
		protected bool IsAdmin => ClassroomRole >= ClassroomRole.Admin;

		/// <summary>
		/// Returns the given user's classroom membership for this classroom.
		/// </summary>
		private static ClassroomMembership GetClassroomMembership(
			User user, 
			Classroom classroom)
		{
			return user?.ClassroomMemberships
				?.SingleOrDefault(m => m.Classroom == classroom);
		}

		/// <summary>
		/// Returns the given user's classroom role for this classroom.
		/// </summary>
		private ClassroomRole GetClassroomRole(User user, Classroom classroom)
		{
			if (user?.SuperUser ?? false)
			{
				return ClassroomRole.Admin;
			}
			else
			{
				var classroomRole = ClassroomMembership?.Role ?? ClassroomRole.None;
				if (!Classroom.IsActive && classroomRole < ClassroomRole.Admin)
				{
					classroomRole = ClassroomRole.None;
				}

				return classroomRole;
			}
		}
	}
}
