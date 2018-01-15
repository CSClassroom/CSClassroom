using System.Collections.Generic;
using CSC.Common.Infrastructure.Extensions;
using CSC.CSClassroom.Model.Communications.ServiceResults;
using Microsoft.AspNetCore.Routing;
using ReflectionIT.Mvc.Paging;

namespace CSC.CSClassroom.WebApp.ViewModels.Message
{
	/// <summary>
	/// The view model for the message list view.
	/// </summary>
	public class IndexViewModel
	{
		/// <summary>
		/// The conversations currently shown to the user.
		/// </summary>
		public PagingList<ConversationInfo> Conversations { get; }
		
		/// <summary>
		/// The page size.
		/// </summary>
		public int PageSize { get; }
		
		/// <summary>
		/// The student ID (if any).
		/// </summary>
		public int? StudentId { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public IndexViewModel(
			PagingList<ConversationInfo> conversations, 
			int pageSize, 
			int? studentId)
		{
			Conversations = conversations;
			PageSize = pageSize;
			StudentId = studentId;

			Conversations.RouteValue = new RouteValueDictionary();
			Conversations.RouteValue[nameof(PageSize).ToCamelCase()] = pageSize;
			if (studentId != null)
			{
				Conversations.RouteValue[nameof(StudentId).ToCamelCase()] = studentId;
			}
		}
	}
}