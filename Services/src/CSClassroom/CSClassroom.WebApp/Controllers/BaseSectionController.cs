using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The base class for controllers managing resources in a section.
	/// </summary>
	public class BaseSectionController : BaseClassroomController
	{
		/// <summary>
		/// The section service.
		/// </summary>
		protected ISectionService SectionService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseSectionController(
			BaseControllerArgs args, 
			IClassroomService classroomService,
			ISectionService sectionService) 
				: base(args, classroomService)
		{
			SectionService = sectionService;
		}

		/// <summary>
		/// The section name route string.
		/// </summary>
		private const string c_sectionNameRouteStr = "sectionName";

		/// <summary>
		/// The section route prefix.
		/// </summary>
		protected const string SectionRoutePrefix = ClassroomRoutePrefix + "/Sections/{" + c_sectionNameRouteStr + "}";

		/// <summary>
		/// The name of the current section.
		/// </summary>
		protected string SectionName => (string)RouteData.Values[c_sectionNameRouteStr];

		/// <summary>
		/// The current section.
		/// </summary>
		public Section Section { get; private set; }

		/// <summary>
		/// The current section membership (if any).
		/// </summary>
		public SectionRole SectionRole { get; private set; }

		/// <summary>
		/// Executes before the action is executed.
		/// </summary>
		protected override async Task InitializeAsync()
		{
			await base.InitializeAsync();

			Section = await SectionService.GetSectionAsync(ClassroomName, SectionName);
			SectionRole = GetSectionRole(User, Section);

			ViewBag.Section = Section;
			ViewBag.SectionRole = SectionRole;
		}

		/// <summary>
		/// Returns whether or not the resource exists.
		/// </summary>
		public override bool DoesResourceExist()
		{
			return Section != null;
		}

		/// <summary>
		/// Returns the given user's role for this section.
		/// </summary>
		private static SectionRole GetSectionRole(User user, Section section)
		{
			if (user?.SuperUser ?? false)
			{
				return SectionRole.Admin;
			}
			else
			{
				var classroomMembership = user?.ClassroomMemberships
					?.SingleOrDefault(m => m.Classroom == section.Classroom);

				if (classroomMembership?.Role == ClassroomRole.Admin)
					return SectionRole.Admin;

				return classroomMembership?.SectionMemberships
					?.SingleOrDefault(m => m.Section == section)
					?.Role ?? SectionRole.None;
			}
		}
	}
}
