using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.QuestionSolvers;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionSolving
{
	/// <summary>
	/// Unit tests for the QuestionStatusCalculator class.
	/// </summary>
	public class QuestionStatusCalculator_UnitTests
	{
		/// <summary>
		/// Ensures that GetQuestionStatus returns the correct number of attempts
		/// the user has completed so far.
		/// </summary>
		[Fact]
		public void GetQuestionStatus_ReturnsCorrectNumAttempts()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numAttempts: 5
			);

			var questionStatusCalculator = new QuestionStatusCalculator();
			var result = questionStatusCalculator.GetQuestionStatus(userQuestionData);

			Assert.Equal(5, result.NumAttempts);
		}
		
		/// <summary>
		/// Ensures that GetQuestionStatus returns the correct value for whether
		/// or not the question was previously answered correctly by the user.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void GetQuestionStatus_ReturnsAnsweredCorrectly(
			bool anyCorrectSubmissions)
		{
			var userQuestionData = CreateUserQuestionData
			(
				anyCorrectSubmissions: anyCorrectSubmissions
			);

			var questionStatusCalculator = new QuestionStatusCalculator();
			var result = questionStatusCalculator.GetQuestionStatus(userQuestionData);

			Assert.Equal(anyCorrectSubmissions, result.AnsweredCorrectly);
		}
		
		/// <summary>
		/// Ensures that GetQuestionStatus returns unlimited remaining attempts for an admin.
		/// </summary>
		[Fact]
		public void GetQuestionStatus_Admin_ReturnsUnlimitedRemainingAttempts()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numAttempts: 1,
				maxAttempts: 1,
				anyCorrectSubmissions: true,
				admin: true,
				combinedSubmissions: false,
				supportsInteractive: false
			);

			var questionStatusCalculator = new QuestionStatusCalculator();
			var result = questionStatusCalculator.GetQuestionStatus(userQuestionData);

			Assert.True(result.AllowNewAttempt);
			Assert.Null(result.NumAttemptsRemaining);
		}
		
		/// <summary>
		/// Ensures that GetQuestionStatus returns no remaining attempts for a single non-interactive
		/// question that was already answered correctly.
		/// </summary>
		[Fact]
		public void GetQuestionStatus_SingleNonInteractiveQuestion_AnsweredCorrectly_ReturnsNoRemainingAttempts()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numAttempts: 1,
				maxAttempts: 10,
				anyCorrectSubmissions: true,
				combinedSubmissions: false,
				supportsInteractive: false
			);

			var questionStatusCalculator = new QuestionStatusCalculator();
			var result = questionStatusCalculator.GetQuestionStatus(userQuestionData);

			Assert.False(result.AllowNewAttempt);
			Assert.Equal(0, result.NumAttemptsRemaining);
		}
		
		/// <summary>
		/// Ensures that GetQuestionStatus returns the correct number of remaining attempts 
		/// for a single interactive question, even if it was already answered correctly.
		/// </summary>
		[Fact]
		public void GetQuestionStatus_SingleInteractiveQuestion_AnsweredCorrectly_ReturnsRemainingAttempts()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numAttempts: 1,
				maxAttempts: 10,
				anyCorrectSubmissions: true,
				combinedSubmissions: false,
				supportsInteractive: true
			);

			var questionStatusCalculator = new QuestionStatusCalculator();
			var result = questionStatusCalculator.GetQuestionStatus(userQuestionData);

			Assert.True(result.AllowNewAttempt);
			Assert.Equal(9, result.NumAttemptsRemaining);
		}
		
		/// <summary>
		/// Ensures that GetQuestionStatus returns the correct number of remaining attempts 
		/// for a non-interactive question in an assignment with combined submissions, even 
		/// if it was already answered correctly.
		/// </summary>
		[Fact]
		public void GetQuestionStatus_CombinedNonInteractiveQuestion_AnsweredCorrectly_ReturnsRemainingAttempts()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numAttempts: 1,
				maxAttempts: 10,
				anyCorrectSubmissions: true,
				combinedSubmissions: true,
				supportsInteractive: false
			);

			var questionStatusCalculator = new QuestionStatusCalculator();
			var result = questionStatusCalculator.GetQuestionStatus(userQuestionData);

			Assert.True(result.AllowNewAttempt);
			Assert.Equal(9, result.NumAttemptsRemaining);
		}
		
		/// <summary>
		/// Ensures that GetQuestionStatus returns the unlimited attempts for a question
		/// that does not have an attempt limit. 
		/// for a non-interactive question in an assignment with combined submissions, even 
		/// if it was already answered correctly.
		/// </summary>
		[Fact]
		public void GetQuestionStatus_NoAttemptLimit_ReturnsUnlimitedRemainingAttempts()
		{
			var userQuestionData = CreateUserQuestionData
			(
				numAttempts: 500,
				maxAttempts: null
			);

			var questionStatusCalculator = new QuestionStatusCalculator();
			var result = questionStatusCalculator.GetQuestionStatus(userQuestionData);

			Assert.True(result.AllowNewAttempt);
			Assert.Null(result.NumAttemptsRemaining);
		}
		
		/// <summary>
		/// Creates a new UserQuestionData object.
		/// </summary>
		protected UserQuestionData CreateUserQuestionData(
			int numAttempts = 0,
			int? maxAttempts = null,
			bool anyCorrectSubmissions = false,
			bool admin = false,
			bool combinedSubmissions = false,
			bool supportsInteractive = false)
		{
			var classroom = new Classroom();
			var user = CreateUser(classroom, admin);
			return new UserQuestionData()
			{
				AssignmentQuestion = new AssignmentQuestion()
				{
					Assignment = new Assignment()
					{
						MaxAttempts = maxAttempts,
						CombinedSubmissions = combinedSubmissions,
						Classroom = classroom
					},
					Question = supportsInteractive
						? (Question)new ClassQuestion()
						: new GeneratedQuestionTemplate()
				},
				NumAttempts = numAttempts,
				User = user,
				Submissions = anyCorrectSubmissions
					? Collections.CreateList
						(
							new UserQuestionSubmission()
							{
								Score = 1.0
							}
						)
					: null
			};
		}

		
		/// <summary>
		/// Creates a new user.
		/// </summary>
		private User CreateUser(Classroom classroom, bool admin)
		{
			return new User()
			{
				ClassroomMemberships = Collections.CreateList
				(
					new ClassroomMembership()
					{
						Classroom = classroom,
						ClassroomId = classroom.Id,
						Role = admin ? ClassroomRole.Admin : ClassroomRole.General
					}
				)
			};
		}
	}
}