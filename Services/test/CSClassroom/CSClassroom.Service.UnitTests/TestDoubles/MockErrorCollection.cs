using System.Collections.Generic;
using CSC.Common.Infrastructure.Utilities;

namespace CSC.CSClassroom.Service.UnitTests.TestDoubles
{
	/// <summary>
	/// A mock error collection.
	/// </summary>
	public class MockErrorCollection : IModelErrorCollection
	{
		/// <summary>
		/// Errors added to the collection.
		/// </summary>
		private readonly IList<KeyValuePair<string, string>> _errors
			= new List<KeyValuePair<string, string>>();

		/// <summary>
		/// Adds an error to the collection.
		/// </summary>
		public void AddError(string propertyName, string errorText)
		{
			_errors.Add(new KeyValuePair<string, string>(propertyName, errorText));
		}

		/// <summary>
		/// Verifies that errors for a given set of property names have been added.
		/// </summary>
		/// <param name="propertyNames"></param>
		public bool VerifyErrors(params string[] propertyNames)
		{
			var errorNames = new HashSet<string>(propertyNames);
			
			return errorNames.SetEquals(propertyNames);
		}

		/// <summary>
		/// Whether or not errors have been added.
		/// </summary>
		public bool HasErrors => _errors.Count > 0;
	}
}
