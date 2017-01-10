using CSC.Common.Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CSC.CSClassroom.WebApp.ModelBinders
{
	/// <summary>
	/// An implementation of a model error collection that is backed by
	/// the request's model state dictionary.
	/// </summary>
	public class ModelErrorCollection : IModelErrorCollection
	{
		/// <summary>
		/// The model state dictionary.
		/// </summary>
		private readonly ModelStateDictionary _modelStateDictionary;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="modelStateDictionary">The model state dictionary.</param>
		public ModelErrorCollection(ModelStateDictionary modelStateDictionary)
		{
			_modelStateDictionary = modelStateDictionary;
		}

		/// <summary>
		/// Adds a model error to the collection.
		/// </summary>
		/// <param name="propertyName">The property name that the error pertains to (if any).</param>
		/// <param name="errorText">The error text.</param>
		public void AddError(string propertyName, string errorText)
		{
			_modelStateDictionary.AddModelError(propertyName, errorText);
		}

		/// <summary>
		/// Returns whether or not there are any errors in the collection.
		/// </summary>
		public bool HasErrors => !_modelStateDictionary.IsValid;
	}
}
