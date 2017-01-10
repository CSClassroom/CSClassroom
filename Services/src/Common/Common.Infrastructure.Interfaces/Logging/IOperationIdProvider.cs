namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Provides the operation ID of the current request.
	/// </summary>
	public interface IOperationIdProvider
	{
		/// <summary>
		/// The operation ID of the current request.
		/// </summary>
		string OperationId { get; }
	}
}
