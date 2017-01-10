namespace CSC.Common.Infrastructure.Utilities
{
	/// <summary>
	/// A collection of model errors.
	/// </summary>
	public interface IModelErrorCollection
	{
		/// <summary>
		/// Adds a model error to the collection.
		/// </summary>
		/// <param name="propertyName">The property name that the error pertains to (if any).</param>
		/// <param name="errorText">The error text.</param>
		void AddError(string propertyName, string errorText);

		/// <summary>
		/// Returns whether or not there are errors.
		/// </summary>
		bool HasErrors { get; }
	}
}
