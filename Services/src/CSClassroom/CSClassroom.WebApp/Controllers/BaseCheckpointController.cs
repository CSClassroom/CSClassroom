using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Projects;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The base class for controllers managing resources for a checkpoint.
	/// </summary>
	public class BaseCheckpointController : BaseProjectController
	{
		/// <summary>
		/// The project service.
		/// </summary>
		protected ICheckpointService CheckpointService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseCheckpointController(
			BaseControllerArgs args, 
			IClassroomService classroomService,
			IProjectService projectService,
			ICheckpointService checkpointService) 
				: base(args, classroomService, projectService)
		{
			CheckpointService = checkpointService;
		}

		/// <summary>
		/// The checkpoint name route string.
		/// </summary>
		private const string c_checkpointNameRouteStr = "checkpointName";

		/// <summary>
		/// The checkpoint route prefix.
		/// </summary>
		protected const string CheckpointRoutePrefix = ProjectRoutePrefix + "/Checkpoints/{" + c_checkpointNameRouteStr + "}";

		/// <summary>
		/// The name of the current checkpoint.
		/// </summary>
		protected string CheckpointName => (string)RouteData.Values[c_checkpointNameRouteStr];

		/// <summary>
		/// The current checkpoint.
		/// </summary>
		public Checkpoint Checkpoint { get; private set; }

		/// <summary>
		/// Executes before the action is executed.
		/// </summary>
		protected override async Task InitializeAsync()
		{
			await base.InitializeAsync();

			Checkpoint = await CheckpointService.GetCheckpointAsync
			(
				ClassroomName, 
				ProjectName, 
				CheckpointName
			);

			ViewBag.Checkpoint = Checkpoint;
		}

		/// <summary>
		/// Returns whether or not the resource exists.
		/// </summary>
		protected override bool DoesResourceExist()
		{
			return Checkpoint != null;
		}
	}
}
