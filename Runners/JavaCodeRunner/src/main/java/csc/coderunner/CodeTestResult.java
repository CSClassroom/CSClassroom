package csc.coderunner;

/**
 * The result of running a test.
 */
public class CodeTestResult
{
	private String name;
	private boolean completed;
	private String exception;
	private String returnValue;
	private String output;
	
	/**
	 * Constructor.
	 * @param name The name of the test.
	 * @param completed Whether or not the test completed successfully.
	 * @param exception The exception thrown by the test, if it did not complete successfully.
	 * @param returnValue The return value of the test, if the test method had a non-void return type.
	 * @param output The output of the test, if any.
	 */
	public CodeTestResult(
		String name, 
		boolean completed, 
		String exception, 
		String returnValue, 
		String output)
	{
		this.name = name;
		this.completed = completed;
		this.exception = exception;
		this.returnValue = returnValue;
		this.output = output;
	}

	/**
	 * @return The name of the test.
	 */
	public String getName()
	{
		return name;
	}

	/**
	 * @return Whether or not the test completed successfully.
	 */
	public boolean getCompleted()
	{
		return completed;
	}
	
	/**
	 * @return The exception thrown by the test, if it did not complete successfully.
	 */
	public String getException()
	{
		return exception;
	}
	
	/**
	 * @return The return value of the test, if the test method had a return type.
	 */
	public String getReturnValue()
	{
		return returnValue;
	}
	
	/**
	 * @return The output of the test, if any.
	 */
	public String getOutput()
	{
		return output;
	}
}
