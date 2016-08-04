namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// A test to run for a method.
	/// </summary>
	public class MethodTest
	{
		/// <summary>
		/// The name of the test.
		/// </summary>
		public string TestName { get; set; }

		/// <summary>
		/// The parameter values to pass to the method.
		/// </summary>
		public string ParamValues { get; set; }
	}
}