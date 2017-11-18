using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.UserQuestionDataLoaders
{
	/// <summary>
	/// Unit tests for the UserQuestionDataStore class.
	/// </summary>
	public class UserQuestionDataStore_UnitTests
	{
		/// <summary>
		/// Ensures that GetUserQuestionData returns the correct object for
		/// the given assignment question ID.
		/// </summary>
		[Fact]
		public void GetUserQuestionData_ReturnsUserQuestionData()
		{
			var uqds = new Dictionary<int, UserQuestionData>()
			{
				[1] = new UserQuestionData()
				{
					Id = 12345
				}
			};

			var store = new UserQuestionDataStore(uqds);
			
			Assert.Equal(12345, store.GetUserQuestionData(1).Id);
		}
		
		/// <summary>
		/// Ensures that GetLoadedAssignmentQuestionIds a list of assignment question,
		/// IDs, ordered by the order property of each AssignmentQuestion.
		/// </summary>
		[Fact]
		public void GetLoadedAssignmentQuestionIds_ReturnsIdsInCorrectOrder()
		{
			var uqds = new Dictionary<int, UserQuestionData>()
			{
				[1] = new UserQuestionData()
				{
					AssignmentQuestion = new AssignmentQuestion()
					{
						Id = 1,
						Order = 2
					}
				},
				[2] = new UserQuestionData()
				{
					AssignmentQuestion = new AssignmentQuestion()
					{
						Id = 2,
						Order = 1
					}
				},
			};

			var store = new UserQuestionDataStore(uqds);

			Assert.Equal(new List<int> {2, 1}, store.GetLoadedAssignmentQuestionIds());
		}
	}
}
