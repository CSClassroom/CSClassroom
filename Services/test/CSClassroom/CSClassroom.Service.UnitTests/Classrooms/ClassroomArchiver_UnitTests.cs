using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Classrooms
{
	/// <summary>
	/// Unit tests for the ClassroomArchiver class.
	/// </summary>
	public class ClassroomArchiver_UnitTests
	{
		/// <summary>
		/// Ensures that ArchiveClassroomAsync marks the old (renamed) classroom
		/// as inactive.
		/// </summary>
		[Fact]
		public async Task ArchiveClassroomAsync_MarksOldClassroomAsInactive()
		{
			var databaseBuilder = GetDatabase();

			var archiver = new ClassroomArchiver(databaseBuilder.Options);
			await archiver.ArchiveClassroomAsync("Class1", "Class1Archived");

			var database = databaseBuilder.Build();

			var oldClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1Archived");

			Assert.False(oldClassroom.IsActive);
		}

		/// <summary>
		/// Ensures that ArchiveClassroomAsync correctly duplicates the relevant
		/// classroom-related entities.
		/// </summary>
		[Fact]
		public async Task ArchiveClassroomAsync_DuplicatesClassroomRelatedEntities()
		{
			var databaseBuilder = GetDatabase();

			var archiver = new ClassroomArchiver(databaseBuilder.Options);
			await archiver.ArchiveClassroomAsync("Class1", "Class1Archived");

			var database = databaseBuilder.Build();

			var oldClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1Archived");

			var newClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1");

			VerifySections(database.Context, oldClassroom, newClassroom);
			VerifyClassroomGradebooks(database.Context, oldClassroom, newClassroom);
		}

		/// <summary>
		/// Ensures that ArchiveClassroomAsync correctly duplicates the relevant
		/// question-related entities.
		/// </summary>
		[Fact]
		public async Task ArchiveClassroomAsync_DuplicatesQuestionRelatedEntities()
		{
			var databaseBuilder = GetDatabase();

			var archiver = new ClassroomArchiver(databaseBuilder.Options);
			await archiver.ArchiveClassroomAsync("Class1", "Class1Archived");

			var database = databaseBuilder.Build();

			var oldClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1Archived");

			var newClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1");

			VerifyQuestionCategories(database.Context, oldClassroom, newClassroom);
		}

		/// <summary>
		/// Ensures that ArchiveClassroomAsync correctly duplicates the relevant
		/// assignment-related entities.
		/// </summary>
		[Fact]
		public async Task ArchiveClassroomAsync_DuplicatesAssignmentRelatedEntities()
		{
			var databaseBuilder = GetDatabase();

			var archiver = new ClassroomArchiver(databaseBuilder.Options);
			await archiver.ArchiveClassroomAsync("Class1", "Class1Archived");

			var database = databaseBuilder.Build();

			var oldClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1Archived");

			var newClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1");

			VerifyAssignments(database.Context, oldClassroom, newClassroom);
		}

		/// <summary>
		/// Ensures that ArchiveClassroomAsync correctly duplicates the relevant
		/// project-related entities.
		/// </summary>
		[Fact]
		public async Task ArchiveClassroomAsync_DuplicatesProjectRelatedEntities()
		{
			var databaseBuilder = GetDatabase();

			var archiver = new ClassroomArchiver(databaseBuilder.Options);
			await archiver.ArchiveClassroomAsync("Class1", "Class1Archived");

			var database = databaseBuilder.Build();

			var oldClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1Archived");

			var newClassroom = database.Context
				.Classrooms
				.Single(c => c.Name == "Class1");

			VerifyProjects(database.Context, oldClassroom, newClassroom);
		}

		/// <summary>
		/// Returns a database.
		/// </summary>
		private TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddSection("Class1", "Period2")
				.AddAdmin("Admin", "AdminLast", "AdminFirst", "Class1", superUser: false)
				.AddStudent("Student", "StudentLast", "StudentFirst", "Class1", "Period1")
				.AddClassroomGradebook("Class1", "ClassroomGradebook")
				.AddSectionGradebook("Class1", "ClassroomGradebook", "Period1", DateTime.MaxValue)
				.AddQuestionCategory("Class1", "Category1")
					.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion()
					{
						Name = "Question1",
						Choices = Collections.CreateList
						(
							new MultipleChoiceQuestionChoice() { Value = "Choice1" },
							new MultipleChoiceQuestionChoice() { Value = "Choice2" }
						)
					})
					.AddQuestion("Class1", "Category1", new ShortAnswerQuestion()
					{
						Name = "Question2",
						Blanks = Collections.CreateList
						(
							new ShortAnswerQuestionBlank() { Name = "Blank1" },
							new ShortAnswerQuestionBlank() { Name = "Blank2" }
						)
					})
				.AddQuestionCategory("Class1", "Category2")
					.AddQuestion("Class1", "Category2", new MethodQuestion()
					{
						Name = "Question3",
						ImportedClasses = Collections.CreateList
						(
							new ImportedClass() { ClassName = "ImportedClass1" },
							new ImportedClass() { ClassName = "ImportedClass2" }
						),
						CodeConstraints = Collections.CreateList
						(
							new CodeConstraint() { Regex = "Regex1" },
							new CodeConstraint() { Regex = "Regex2" }
						),
						Tests = Collections.CreateList
						(
							new MethodQuestionTest() { ParameterValues = "Params1" },
							new MethodQuestionTest() { ParameterValues = "Params2" }
						)
					})
					.AddQuestion("Class1", "Category2", new ClassQuestion()
					{
						Name = "Question4",
						ImportedClasses = Collections.CreateList
						(
							new ImportedClass() { ClassName = "ImportedClass1" },
							new ImportedClass() { ClassName = "ImportedClass2" }
						),
						CodeConstraints = Collections.CreateList
						(
							new CodeConstraint() { Regex = "Regex1" },
							new CodeConstraint() { Regex = "Regex2" }
						),
						Tests = Collections.CreateList
						(
							new ClassQuestionTest() { Description = "Description1" },
							new ClassQuestionTest() { Description = "Description2" }
						),
						RequiredMethods = Collections.CreateList
						(
							new RequiredMethod() { Name = "RequiredMethod1" },
							new RequiredMethod() { Name = "RequiredMethod2" }
						)
					})
					.AddQuestion("Class1", "Category2", new ProgramQuestion()
					{
						Name = "Question5",
						ImportedClasses = Collections.CreateList
						(
							new ImportedClass() { ClassName = "ImportedClass1" },
							new ImportedClass() { ClassName = "ImportedClass2" }
						),
						CodeConstraints = Collections.CreateList
						(
							new CodeConstraint() { Regex = "Regex1" },
							new CodeConstraint() { Regex = "Regex2" }
						),
						Tests = Collections.CreateList
						(
							new ProgramQuestionTest() { TestDescription = "TestDescription1" },
							new ProgramQuestionTest() { TestDescription = "TestDescription2" }
						)
					})
					.AddQuestion("Class1", "Category2", new GeneratedQuestionTemplate()
					{
						Name = "Question6"
					})
					.AddQuestion("Class1", "Category2", new RandomlySelectedQuestion()
					{
						Name = "Question7",
						ChoicesCategory = new QuestionCategory()
						{
							Name = "Choices Category",
							Questions = Collections.CreateList
							(
								(Question)new MultipleChoiceQuestion()
								{
									Name = "Choice1",
									Choices = Collections.CreateList
									(
										new MultipleChoiceQuestionChoice() { Value = "Choice1" },
										new MultipleChoiceQuestionChoice() { Value = "Choice2" }
									)
								},
								new ShortAnswerQuestion()
								{
									Name = "Choice2",
									Blanks = Collections.CreateList
									(
										new ShortAnswerQuestionBlank() { Name = "Blank1" },
										new ShortAnswerQuestionBlank() { Name = "Blank2" }
									)
								}
							)
						}
					})
				.AddAssignment("Class1", "Unit 1", "Unit 1a",
					sectionDueDates: new Dictionary<string, DateTime>()
					{
						["Period1"] = DateTime.MinValue,
						["Period2"] = DateTime.MaxValue
					},
					questionsByCategory: new Dictionary<string, string[]>()
					{
						["Category1"] = new [] { "Question1" },
						["Category2"] = new [] { "Question3", "Question4" }
					})
				.AddAssignment("Class1", "Unit 1", "Unit 1b",
					sectionDueDates: new Dictionary<string, DateTime>()
					{
						["Period1"] = DateTime.MinValue,
						["Period2"] = DateTime.MaxValue
					},
					questionsByCategory: new Dictionary<string, string[]>()
					{
						["Category1"] = new[] { "Question2" },
						["Category2"] = new[] { "Question5", "Question6", "Question7" }
					})
				.AddProject("Class1", "Project1")
					.AddProjectPath("Class1", "Project1", "Path1", FileType.Private)
					.AddProjectPath("Class1", "Project1", "Path2", FileType.Immutable)
					.AddProjectTestClass("Class1", "Project1", "TestClass1")
					.AddProjectTestClass("Class1", "Project1", "TestClass2")
					.AddCommit("Class1", "Project1", "Student", "Sha1", DateTime.MinValue)
					.AddCommit("Class1", "Project1", "Student", "Sha2", DateTime.MaxValue)
					.AddCheckpoint("Class1", "Project1", "Checkpoint1")
						.AddCheckpointTestClass("Class1", "Project1", "Checkpoint1", "TestClass1", required: true)
						.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period1", DateTime.MinValue)
						.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period2", DateTime.MaxValue)
						.AddSubmission("Class1", "Project1", "Checkpoint1", "Student", "Sha1", DateTime.MinValue)
					.AddCheckpoint("Class1", "Project1", "Checkpoint2")
						.AddCheckpointTestClass("Class1", "Project1", "Checkpoint2", "TestClass1", required: true)
						.AddCheckpointTestClass("Class1", "Project1", "Checkpoint2", "TestClass2", required: true)
						.AddCheckpointDueDate("Class1", "Project1", "Checkpoint2", "Period1", DateTime.MinValue)
						.AddCheckpointDueDate("Class1", "Project1", "Checkpoint2", "Period2", DateTime.MaxValue)
						.AddSubmission("Class1", "Project1", "Checkpoint2", "Student", "Sha2", DateTime.MaxValue);
		}

		/// <summary>
		/// Verifies that the sections in the given classroom were not duplicated.
		/// </summary>
		private void VerifySections(
			DatabaseContext dbContext, 
			Classroom oldClassroom, 
			Classroom newClassroom)
		{
			VerifyCollectionNotDuplicated
			(
				dbContext,
				oldClassroom,
				newClassroom,
				c => c.Sections
			);
		}

		/// <summary>
		/// Verifies that the question categories for the given classroom
		/// were correctly duplicated.
		/// </summary>
		private void VerifyQuestionCategories(
			DatabaseContext dbContext,
			Classroom oldClassroom, 
			Classroom newClassroom)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldClassroom,
				newClassroom,
				c => c.Categories,
				q => q.Id,
				q => q.Name,
				VerifyQuestionCategory
			);
		}

		/// <summary>
		/// Verifies that the given question category was correctly duplicated.
		/// </summary>
		private void VerifyQuestionCategory(
			DatabaseContext dbContext,
			QuestionCategory oldCategory, 
			QuestionCategory newCategory)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldCategory,
				newCategory,
				qc => qc.Questions,
				q => q.Id,
				q => q.Name,
				VerifyQuestion
			);
		}

		/// <summary>
		/// Verifies that the given question was correctly duplicated.
		/// </summary>
		private void VerifyQuestion(
			DatabaseContext dbContext, 
			Question oldQuestion, 
			Question newQuestion)
		{
			if (oldQuestion is MultipleChoiceQuestion)
			{
				VerifyMultipleChoiceQuestion
				(
					dbContext,
					(MultipleChoiceQuestion)oldQuestion, 
					(MultipleChoiceQuestion)newQuestion
				);
			}
			else if (oldQuestion is ShortAnswerQuestion)
			{
				VerifyShortAnswerQuestion
				(
					dbContext,
					(ShortAnswerQuestion)oldQuestion,
					(ShortAnswerQuestion)newQuestion
				);
			}
			else if (oldQuestion is MethodQuestion)
			{
				VerifyMethodQuestion
				(
					dbContext,
					(MethodQuestion)oldQuestion,
					(MethodQuestion)newQuestion
				);
			}
			else if (oldQuestion is ClassQuestion)
			{
				VerifyClassQuestion
				(
					dbContext,
					(ClassQuestion)oldQuestion,
					(ClassQuestion)newQuestion
				);
			}
			else if (oldQuestion is ProgramQuestion)
			{
				VerifyProgramQuestion
				(
					dbContext,
					(ProgramQuestion)oldQuestion,
					(ProgramQuestion)newQuestion
				);
			}
			else if (oldQuestion is GeneratedQuestionTemplate)
			{
				VerifyGeneratedQuestionTemplate
				(
					dbContext,
					(GeneratedQuestionTemplate)oldQuestion,
					(GeneratedQuestionTemplate)newQuestion
				);
			}
			else if (oldQuestion is RandomlySelectedQuestion)
			{
				VerifyRandomlySelectedQuestion
				(
					dbContext,
					(RandomlySelectedQuestion)oldQuestion,
					(RandomlySelectedQuestion)newQuestion
				);
			}
			else
			{
				throw new InvalidOperationException("Unknown question type.");
			}
		}

		/// <summary>
		/// Verifies that the given multiple choice question was correctly duplicated.
		/// </summary>
		private void VerifyMultipleChoiceQuestion(
			DatabaseContext dbContext,
			MultipleChoiceQuestion oldQuestion,
			MultipleChoiceQuestion newQuestion)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.Choices,
				b => b.Id,
				b => b.Value
			);
		}

		/// <summary>
		/// Verifies that the given short answer question was correctly duplicated.
		/// </summary>
		private void VerifyShortAnswerQuestion(
			DatabaseContext dbContext,
			ShortAnswerQuestion oldQuestion,
			ShortAnswerQuestion newQuestion)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.Blanks,
				b => b.Id,
				b => b.Name
			);
		}

		/// <summary>
		/// Verifies that the given code question was correctly duplicated.
		/// </summary>
		private void VerifyCodeQuestion(
			DatabaseContext dbContext,
			CodeQuestion oldQuestion,
			CodeQuestion newQuestion)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.ImportedClasses,
				c => c.Id,
				c => c.ClassName
			);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.CodeConstraints,
				c => c.Id,
				c => c.Regex
			);
		}

		/// <summary>
		/// Verifies that the given method question was correctly duplicated.
		/// </summary>
		private void VerifyMethodQuestion(
			DatabaseContext dbContext,
			MethodQuestion oldQuestion,
			MethodQuestion newQuestion)
		{
			VerifyCodeQuestion(dbContext, oldQuestion, newQuestion);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.Tests,
				t => t.Id,
				t => t.ParameterValues
			);
		}

		/// <summary>
		/// Verifies that the given class question was correctly duplicated.
		/// </summary>
		private void VerifyClassQuestion(
			DatabaseContext dbContext,
			ClassQuestion oldQuestion,
			ClassQuestion newQuestion)
		{
			VerifyCodeQuestion(dbContext, oldQuestion, newQuestion);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.Tests,
				t => t.Id,
				t => t.Description
			);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.RequiredMethods,
				r => r.Id,
				r => r.Name
			);
		}

		/// <summary>
		/// Verifies that the given program question was correctly duplicated.
		/// </summary>
		private void VerifyProgramQuestion(
			DatabaseContext dbContext,
			ProgramQuestion oldQuestion,
			ProgramQuestion newQuestion)
		{
			VerifyCodeQuestion(dbContext, oldQuestion, newQuestion);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.Tests,
				t => t.Id,
				t => t.TestDescription
			);
		}

		/// <summary>
		/// Verifies that the given generated question template was 
		/// correctly duplicated.
		/// </summary>
		private void VerifyGeneratedQuestionTemplate(
			DatabaseContext dbContext,
			GeneratedQuestionTemplate oldQuestion,
			GeneratedQuestionTemplate newQuestion)
		{
			// No navigation properties.
		}

		/// <summary>
		/// Verifies that the given randomly selected question was 
		/// correctly duplicated.
		/// </summary>
		private void VerifyRandomlySelectedQuestion(
			DatabaseContext dbContext,
			RandomlySelectedQuestion oldQuestion,
			RandomlySelectedQuestion newQuestion)
		{
			VerifyReferenceDuplicated
			(
				dbContext,
				oldQuestion,
				newQuestion,
				q => q.ChoicesCategory,
				q => q.Id,
				q => q.Name,
				VerifyQuestionCategory
			);
		}

		/// <summary>
		/// Verifies that the assignments in the given classroom were 
		/// correctly duplicated.
		/// </summary>
		private void VerifyAssignments(
			DatabaseContext dbContext,
			Classroom oldClassroom,
			Classroom newClassroom)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldClassroom,
				newClassroom,
				c => c.Assignments,
				a => a.Id,
				a => a.Name,
				VerifyAssignment
			);
		}

		/// <summary>
		/// Verifies that the given assignment was correctly duplicated.
		/// </summary>
		private void VerifyAssignment(
			DatabaseContext dbContext,
			Assignment oldAssignment, 
			Assignment newAssignment)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldAssignment, 
				newAssignment,
				q => q.Questions,
				aq => aq.Id, 
				aq => aq.Name,
				VerifyAssignmentQuestion
			);
		}
		
		/// <summary>
		/// Verifies that the given assignment question was correctly duplicated.
		/// </summary>
		private void VerifyAssignmentQuestion(
			DatabaseContext dbContext,
			AssignmentQuestion oldAssignmentQuestion,
			AssignmentQuestion newAssignmentQuestion)
		{
			VerifyReferenceDuplicated
			(
				dbContext,
				oldAssignmentQuestion,
				newAssignmentQuestion,
				aq => aq.Question,
				q => q.Id,
				q => q.Name
			);
		}

		/// <summary>
		/// Verifies that the projects in the given classroom were 
		/// correctly duplicated.
		/// </summary>
		private void VerifyProjects(
			DatabaseContext dbContext,
			Classroom oldClassroom,
			Classroom newClassroom)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldClassroom,
				newClassroom,
				c => c.Projects,
				p => p.Id,
				p => p.Name,
				VerifyProject
			);
		}

		/// <summary>
		/// Verifies that the given project was correctly duplicated.
		/// </summary>
		private void VerifyProject(
			DatabaseContext dbContext,
			Project oldProject,
			Project newProject)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldProject,
				newProject,
				p => p.PrivateFilePaths,
				p => p.Id,
				p => p.Path
			);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldProject,
				newProject,
				p => p.ImmutableFilePaths,
				p => p.Id,
				p => p.Path
			);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldProject,
				newProject,
				p => p.TestClasses,
				p => p.Id,
				p => p.ClassName
			);

			VerifyCollectionDuplicated
			(
				dbContext,
				oldProject,
				newProject,
				p => p.Checkpoints,
				p => p.Id,
				p => p.Name,
				VerifyCheckpoint
			);
		}

		/// <summary>
		/// Verifies that the checkpoints were correctly duplicated.
		/// </summary>
		private void VerifyCheckpoint(
			DatabaseContext dbContext,
			Checkpoint oldCheckpoint,
			Checkpoint newCheckpoint)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldCheckpoint,
				newCheckpoint,
				c => c.TestClasses,
				tc => tc.Id,
				tc => tc.TestClass.ClassName
			);

			VerifyCollectionNotDuplicated
			(
				dbContext,
				oldCheckpoint,
				newCheckpoint,
				c => c.SectionDates
			);

			VerifyCollectionNotDuplicated
			(
				dbContext,
				oldCheckpoint,
				newCheckpoint,
				c => c.Submissions
			);
		}

		/// <summary>
		/// Verifies that the classroom gradebooks for the given classroom
		/// were correctly duplicated.
		/// </summary>
		private void VerifyClassroomGradebooks(
			DatabaseContext dbContext,
			Classroom oldClassroom,
			Classroom newClassroom)
		{
			VerifyCollectionDuplicated
			(
				dbContext,
				oldClassroom,
				newClassroom,
				c => c.ClassroomGradebooks,
				cg => cg.Id,
				cg => cg.Name,
				VerifySectionGradebooks
			);
		}

		/// <summary>
		/// Verifies that the section gradebooks in the classroom gradebook 
		/// were not duplciated.
		/// </summary>
		private void VerifySectionGradebooks(
			DatabaseContext dbContext,
			ClassroomGradebook oldClassroomGradebook,
			ClassroomGradebook newClassroomGradebook)
		{
			VerifyCollectionNotDuplicated
			(
				dbContext,
				oldClassroomGradebook,
				newClassroomGradebook,
				cg => cg.SectionGradebooks
			);
		}

		/// <summary>
		/// Verifies that a given collection of objects was correctly duplicated.
		/// </summary>
		private void VerifyCollectionDuplicated<TParent, TChild>(
			DatabaseContext dbContext,
			TParent oldParent,
			TParent newParent,
			Expression<Func<TParent, IEnumerable<TChild>>> collectionExpr,
			Func<TChild, int> idExpr,
			Func<TChild, string> uniqueKeyExpr,
			Action<DatabaseContext, TChild, TChild> verifyChildrenAction = null)
				where TParent : class
				where TChild : class
		{
			dbContext.Entry(oldParent).Collection(collectionExpr).Load();
			dbContext.Entry(newParent).Collection(collectionExpr).Load();

			var oldCollection = dbContext.Entry(oldParent)
				.Collection(collectionExpr)
				.CurrentValue
				.ToList();

			var newCollection = dbContext.Entry(newParent)
				.Collection(collectionExpr)
				.CurrentValue
				.ToList();

			Assert.True(oldCollection.Count > 0);
			Assert.Equal(oldCollection.Count, newCollection.Count);

			var keys = oldCollection.Select(uniqueKeyExpr);
			foreach (var key in keys)
			{
				var oldEntity = oldCollection
					.Single(entity => uniqueKeyExpr(entity) == key);

				var newEntity = newCollection
					.Single(entity => uniqueKeyExpr(entity) == key);

				Assert.NotEqual(idExpr(oldEntity), idExpr(newEntity));

				verifyChildrenAction?.Invoke(dbContext, oldEntity, newEntity);
			}
		}

		/// <summary>
		/// Verifies that a given collection of objects was NOT duplicated.
		/// </summary>
		private void VerifyCollectionNotDuplicated<TParent, TChild>(
			DatabaseContext dbContext,
			TParent oldParent,
			TParent newParent,
			Expression<Func<TParent, IEnumerable<TChild>>> collectionExpr)
				where TParent : class
				where TChild : class
		{
			dbContext.Entry(oldParent).Collection(collectionExpr).Load();
			dbContext.Entry(newParent).Collection(collectionExpr).Load();

			var oldCollection = dbContext.Entry(oldParent)
				.Collection(collectionExpr)
				.CurrentValue
				.ToList();

			var newCollection = dbContext.Entry(newParent)
				.Collection(collectionExpr)
				.CurrentValue
				.ToList();

			Assert.True(oldCollection.Count > 0);
			Assert.Empty(newCollection);
		}

		/// <summary>
		/// Verifies that a given collection of objects was correctly duplicated.
		/// </summary>
		private void VerifyReferenceDuplicated<TParent, TChild>(
			DatabaseContext dbContext,
			TParent oldParent,
			TParent newParent,
			Expression<Func<TParent, TChild>> referenceExpr,
			Func<TChild, int> idExpr,
			Func<TChild, string> uniqueKeyExpr,
			Action<DatabaseContext, TChild, TChild> verifyChildrenAction = null)
				where TParent : class
				where TChild : class
		{
			dbContext.Entry(oldParent).Reference(referenceExpr).Load();
			dbContext.Entry(newParent).Reference(referenceExpr).Load();

			var oldReference = dbContext.Entry(oldParent)
				.Reference(referenceExpr)
				.CurrentValue;

			var newReference = dbContext.Entry(newParent)
				.Reference(referenceExpr)
				.CurrentValue;

			Assert.NotEqual(idExpr(oldReference), idExpr(newReference));
			Assert.Equal(uniqueKeyExpr(oldReference), uniqueKeyExpr(newReference));

			verifyChildrenAction?.Invoke(dbContext, oldReference, newReference);
		}
	}
}
