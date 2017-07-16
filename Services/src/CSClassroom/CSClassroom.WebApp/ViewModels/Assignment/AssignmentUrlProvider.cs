using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Provides URLs related to an assignment.
	/// </summary>
	public class AssignmentUrlProvider : IAssignmentUrlProvider
	{
		/// <summary>
		/// The URL helper to generate question URLs.
		/// </summary>
		private readonly IUrlHelper _urlHelper;

		/// <summary>
		/// Whether or not the current user is an admin.
		/// </summary>
		private readonly bool _admin;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentUrlProvider(IUrlHelper urlHelper, bool admin)
		{
			_urlHelper = urlHelper;
			_admin = admin;
		}

		/// <summary>
		/// Returns the assignment URL for the given assignment ID.
		/// </summary>
		public string GetAssignmentUrl(int assignmentId, int userId)
		{
			object routeValues = _admin
				? (object)new { assignmentId, userId }
				: (object)new { assignmentId };
			
			return _urlHelper.Action
			(
				"SolveAll", 
				"AssignmentQuestion",
				routeValues
			);
		}

		/// <summary>
		/// Returns the URL for an assignment submission.
		/// </summary>
		public string GetAssignmentSubmissionUrl(
			int assignmentId,
			DateTime submissionDate,
			int userId)
		{
			object routeValues = _admin
				? (object)new { assignmentId, submissionDate = submissionDate.ToEpoch(), userId }
				: (object)new { assignmentId, submissionDate = submissionDate.ToEpoch() };

			return _urlHelper.Action
			(
				"ViewAllSubmissions",
				"AssignmentQuestion",
				routeValues
			);
		}

		/// <summary>
		/// Returns the URL of the question.
		/// </summary>
		public string GetQuestionUrl(StudentQuestionResult result)
		{
			object routeValues = _admin
				? (object)new
					{
						assignmentId = result.AssignmentId,
						id = result.QuestionId,
						userId = result.UserId
					}
				: (object)new
					{
						assignmentId = result.AssignmentId,
						id = result.QuestionId
					};

			if (result.CombinedSubmissions)
			{
				return null;
			}
			else
			{
				return _urlHelper.Action
				(
					"Solve",
					"AssignmentQuestion",
					routeValues
				);
			}
		}

		/// <summary>
		/// Returns the URL for a question submission.
		/// </summary>
		public string GetQuestionSubmissionUrl(
			int assignmentId,
			int questionId,
			DateTime submissionDate,
			int userId)
		{
			object routeValues = _admin
				? (object)new
				{
					assignmentId,
					id = questionId,
					submissionDate = submissionDate.ToEpoch(),
					userId
				}
				: (object)new
				{
					assignmentId,
					id = questionId,
					submissionDate = submissionDate.ToEpoch()
				};

			return _urlHelper.Action
			(
				"ViewSubmission",
				"AssignmentQuestion",
				routeValues
			);
		}
	}
}
