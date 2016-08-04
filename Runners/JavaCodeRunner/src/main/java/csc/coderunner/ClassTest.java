package csc.coderunner;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * A test to run for a java class.
 */
public class ClassTest extends CodeTest
{
	private String returnType;
	private String methodBody;

	/**
	 * Constructor.
	 * @param testName The name of the test.
	 * @param returnType The return type of the test method.
	 * @param methodBody The body of the test method.
	 */
	public ClassTest(
		@JsonProperty("testName") String testName,
		@JsonProperty("returnType") String returnType,
		@JsonProperty("methodBody") String methodBody)
	{
		super(testName);
		this.returnType = returnType;
		this.methodBody = methodBody;
	}
	
	/**
	 * @param javaClass The class containing the code that will be tested.
	 * @return The return type of the test.
	 */
	protected String getTestMethodReturnType(Class<?> javaClass)
	{
		return returnType;
	}

	/**
	 * @param javaClass The class containing the code that will be tested.
	 * @return The test method body.
	 */
	@Override
	protected String getTestMethodBody(Class<?> javaClass)
	{
		return methodBody;
	}
	
	/**
	 * @return Whether or not to include the class name in stack traces.
	 */
	@Override
	protected boolean includeClassNameInStackTraces()
	{
		return true;
	}
}