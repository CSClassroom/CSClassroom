using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Projects;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The base class for controllers managing resources for a project.
	/// </summary>
	public class BaseProjectController : BaseClassroomController
	{
		/// <summary>
		/// The project service.
		/// </summary>
		protected IProjectService ProjectService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseProjectController(
			BaseControllerArgs args, 
			IClassroomService classroomService,
			IProjectService projectService) 
				: base(args, classroomService)
		{
			ProjectService = projectService;
		}

		/// <summary>
		/// The project name route string.
		/// </summary>
		private const string c_projectNameRouteStr = "projectName";

		/// <summary>
		/// The project route prefix.
		/// </summary>
		protected const string ProjectRoutePrefix = ClassroomRoutePrefix + "/Projects/{" + c_projectNameRouteStr + "}";

		/// <summary>
		/// The name of the current project.
		/// </summary>
		protected string ProjectName => (string)RouteData.Values[c_projectNameRouteStr];

		/// <summary>
		/// The current project.
		/// </summary>
		public Project Project { get; private set; }

		/// <summary>
		/// Executes before the action is executed.
		/// </summary>
		protected override async Task InitializeAsync()
		{
			await base.InitializeAsync();

			Project = await ProjectService.GetProjectAsync(ClassroomName, ProjectName);

			ViewBag.Project = Project;
		}

		/// <summary>
		/// Returns whether or not the resource exists.
		/// </summary>
		public override bool DoesResourceExist()
		{
			return Project != null;
		}
	}
}
