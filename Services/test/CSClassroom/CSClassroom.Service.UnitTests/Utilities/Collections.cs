using System.Collections.Generic;

namespace CSC.CSClassroom.Service.UnitTests.Utilities
{
	/// <summary>
	/// Utility methods for collections.
	/// </summary>
	public static class Collections
	{
		/// <summary>
		/// Creates a list with one item.
		/// </summary>
		public static List<TItem> CreateList<TItem>(params TItem[] items)
		{
			var list = new List<TItem>();
			list.AddRange(items);

			return list;
		}
	}
}
