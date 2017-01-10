namespace CSC.BuildService.Model.CodeRunner
{
	/// <summary>
	/// The result of a method job.
	/// </summary>
	public class MethodJobResult : CodeJobResult
	{
		/// <summary>
		/// The defintion of the compiled method, if there was exactly one method 
		/// and it compiled successfully.
		/// </summary>
		public MethodDefinition MethodDefinition { get; set; }
	}
}
