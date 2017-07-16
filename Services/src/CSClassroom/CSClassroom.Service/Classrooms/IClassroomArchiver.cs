using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Archives a classroom.
	/// </summary>
	public interface IClassroomArchiver
	{
		/// <summary>
		/// Archives the given classroom. This renames the current classroom
		/// and creates a new classroom with the current name. The new
		/// classroom contains projects and assignments, but no section
		/// or user specific data.
		/// </summary>
		Task ArchiveClassroomAsync(
			string curClassroomName,
			string archivedClassroomName);
	}
}
