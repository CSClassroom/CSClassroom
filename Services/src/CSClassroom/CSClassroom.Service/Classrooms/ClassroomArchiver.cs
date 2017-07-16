using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Archives a classroom.
	/// </summary>
	public class ClassroomArchiver : IClassroomArchiver
	{
		/// <summary>
		/// The database context used to load objects from the database.
		/// </summary>
		private readonly DatabaseContext _readDbContext;

		/// <summary>
		/// The database context used to write new objects to the database.
		/// </summary>
		private readonly DatabaseContext _writeDbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassroomArchiver(DbContextOptions<DatabaseContext> dbContextOptions)
		{
			_readDbContext = new DatabaseContext(dbContextOptions);
			_writeDbContext = new DatabaseContext(dbContextOptions);
		}

		/// <summary>
		/// Archives the given classroom. This renames the current classroom
		/// and creates a new classroom with the current name. The new
		/// classroom contains projects and assignments, but no section
		/// or user specific data.
		/// </summary>
		public async Task ArchiveClassroomAsync(
			string curClassroomName,
			string archivedClassroomName)
		{
			await DuplicateObjectsAsync(curClassroomName);
			await ArchiveExistingClassroomAsync(curClassroomName, archivedClassroomName);
			await _writeDbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Duplicates all relevant objects associated iwth the classroom.
		/// </summary>
		private async Task DuplicateObjectsAsync(string curClassroomName)
		{
			var objectsToDuplicate = await LoadObjectsToDuplicateAsync(curClassroomName);

			DuplicateObjects(objectsToDuplicate);
		}

		/// <summary>
		/// Loads all objects associated with the given classroom that we
		/// will duplciate.
		/// </summary>
		private async Task<IList<object>> LoadObjectsToDuplicateAsync(
			string classroomName)
		{
			var objects = new List<object>();

			// Classroom-related objects
			await LoadAsync(_readDbContext.Classrooms, c => c.Name == classroomName, objects);
			await LoadAsync(_readDbContext.ClassroomGradebooks, c => c.Classroom.Name == classroomName, objects);

			// Question-related objects
			await LoadAsync(_readDbContext.QuestionCategories, c => c.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.Questions, q => q.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.MultipleChoiceQuestionChoices, c => c.MultipleChoiceQuestion.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.ShortAnswerQuestionBlanks, b => b.ShortAnswerQuestion.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.ImportedClasses, i => i.CodeQuestion.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.CodeConstraints, c => c.CodeQuestion.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.RequiredMethods, r => r.ClassQuestion.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.ClassQuestionTests, t => t.ClassQuestion.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.MethodQuestionTests, t => t.MethodQuestion.QuestionCategory.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.ProgramQuestionTests, t => t.ProgramQuestion.QuestionCategory.Classroom.Name == classroomName, objects);

			// Assignment-related objects
			await LoadAsync(_readDbContext.Assignments, a => a.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.AssignmentQuestions, a => a.Assignment.Classroom.Name == classroomName, objects);

			// Project-related objects
			await LoadAsync(_readDbContext.Projects, p => p.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.PrivateFilePaths, p => p.Project.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.ImmutableFilePaths, p => p.Project.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.TestClasses, t => t.Project.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.Checkpoints, c => c.Project.Classroom.Name == classroomName, objects);
			await LoadAsync(_readDbContext.CheckpointTestClasses, t => t.Checkpoint.Project.Classroom.Name == classroomName, objects);

			return objects;
		}

		/// <summary>
		/// Loads entities in a table that match the given predicate.
		/// </summary>
		private async Task LoadAsync<TEntity>(
			DbSet<TEntity> table,
			Expression<Func<TEntity, bool>> entityPredicate,
			List<object> objectsToDuplicate) 
				where TEntity : class
		{
			var loadedObjects = await table
				.Where(entityPredicate)
				.ToListAsync();
			
			objectsToDuplicate.AddRange(loadedObjects);
		}

		/// <summary>
		/// Duplicates the given object.
		/// </summary>
		private void DuplicateObjects(IList<object> objectsToDuplicate)
		{
			var uniqueObjectsToDuplicate = objectsToDuplicate.ToHashSet();

			foreach (var entity in uniqueObjectsToDuplicate)
			{
				var idProps = entity.GetType()
					.GetProperties()
					.Where(p => p.PropertyType == typeof(int))
					.Where(p => p.Name.EndsWith("Id"))
					.ToList();

				foreach (var prop in idProps)
				{
					prop.SetValue(entity, 0);
				}
			}

			foreach (var entity in uniqueObjectsToDuplicate)
			{
				_writeDbContext.Add(entity);
			}
		}

		/// <summary>
		/// Renames the existing classroom.
		/// </summary>
		private async Task ArchiveExistingClassroomAsync(
			string curClassroomName,
			string archivedClassroomName)
		{
			var curClassroom = await _writeDbContext.Classrooms
				.SingleAsync(c => c.Name == curClassroomName);

			curClassroom.Name = archivedClassroomName;
			curClassroom.IsActive = false;
		}
	}
}
