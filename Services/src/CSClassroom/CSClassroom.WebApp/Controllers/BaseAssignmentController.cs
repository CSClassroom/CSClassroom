using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.Service.Assignments;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The base class for controllers managing resources for an assignment.
	/// </summary>
	public class BaseAssignmentController : BaseClassroomController
	{
		/// <summary>
		/// The assignment service.
		/// </summary>
		protected IAssignmentService AssignmentService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseAssignmentController(
			BaseControllerArgs args, 
			IClassroomService classroomService,
			IAssignmentService assignmentService) 
				: base(args, classroomService)
		{
			AssignmentService = assignmentService;
		}

		/// <summary>
		/// The checkpoint name route string.
		/// </summary>
		private const string c_assignmentIdRouteStr = "assignmentId";

		/// <summary>
		/// The checkpoint route prefix.
		/// </summary>
		protected const string AssignmentRoutePrefix = ClassroomRoutePrefix + "/Assignments/{" + c_assignmentIdRouteStr + "}";

		/// <summary>
		/// The ID of the current assignment.
		/// </summary>
		protected int AssignmentId => int.Parse((string)RouteData.Values[c_assignmentIdRouteStr]);

		/// <summary>
		/// The current assignment.
		/// </summary>
		protected Assignment Assignment { get; private set; }

		/// <summary>
		/// Executes before the action is executed.
		/// </summary>
		protected override async Task InitializeAsync()
		{
			await base.InitializeAsync();

			Assignment = await AssignmentService.GetAssignmentAsync
			(
				ClassroomName, 
				AssignmentId
			);

			ViewBag.Assignment = Assignment;
		}

		/// <summary>
		/// Returns whether or not the resource exists.
		/// </summary>
		protected override bool DoesResourceExist()
		{
			return Assignment != null;
		}
	}
}
