package csc.coderunner;

import java.io.ByteArrayOutputStream;
import java.io.PrintStream;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.math.BigDecimal;
import java.text.DecimalFormat;
import java.util.Arrays;
/**
 * A test to run for a java class.
 */
public abstract class CodeTest
{
	private static final String c_additionalContentOmitted = "Additional content omitted.";
	private String testName;
	private final int maxSize = 100000;

	/**
	 * Constructor.
	 * @param testName The name of the test.=
	 */
	public CodeTest(String testName)
	{
		this.testName = testName;
	}
	
	/**
	 * @param javaClass The class containing the code that will be tested.
	 * @return The return type of the test.
	 */
	protected abstract String getTestMethodReturnType(Class<?> javaClass);
	
	/**
	 * @param javaClass The class containing the code that will be tested.
	 * @return The test method body.
	 */
	protected abstract String getTestMethodBody(Class<?> javaClass);
	
	/**
	 * @return Whether or not to include the class name in stack traces.
	 */
	protected abstract boolean includeClassNameInStackTraces();

	/**
	 * @return The name of the test.
	 */
	public String getTestName()
	{
		return testName;
	}
	
	/**
	 * @param javaClass The class containing the code that will be tested.
	 * @return A method spec that can be used to generate java code for this test.
	 */
	public void addTestMethod(JavaFileBuilder builder, Class<?> javaClass)
	{
		String returnType = getTestMethodReturnType(javaClass);
		String methodName = getTestName();
		
		builder.addLine("public static %s %s()", returnType, methodName)
			.beginScope()
				.addLines(getTestMethodBody(javaClass))
			.endScope();
	}
	
	/**
	 * Executes this test on the given compiled java class.
	 * @param className The name of the class being tested.
	 * @param javaClass A compiled class that contains this test.
	 * @param lineNumberOffset The line number offset, for stack traces.
	 * @return The test result.
	 */
	public CodeTestResult runTest(
		String className, 
		Class<?> javaTestClass, 
		int lineNumberOffset)
	{
		Method method = null;
		
		try
		{
			method = javaTestClass.getDeclaredMethod(testName);
		} 
		catch (NoSuchMethodException | SecurityException e)
		{
			// This should never happen, since we generated the method.
			throw new RuntimeException(e);
		}
		
		PrintStream oldOutStream = System.out;
		
		try
		{
			ByteArrayOutputStream newOutStream = new ByteArrayOutputStream();
			System.setOut(new PrintStream(newOutStream));
			
			Object returnValue = method.invoke(null);
			String returnString = getReturnValueAsString(returnValue);
			if (returnString != null && returnString.length() > maxSize)
			{
				returnString = returnString.substring(0, maxSize) 
						+ "\n"
						+ c_additionalContentOmitted;
			}
			
			String outputString = newOutStream.toString();
			if (outputString != null && outputString.length() > maxSize)
			{
				outputString = outputString.substring(0, maxSize) 
						+ "\n"
						+ c_additionalContentOmitted;
			}
			
			return new CodeTestResult(
				testName,
				true /*succeeded*/,
				null /*exception*/,
				returnString,
				!outputString.isEmpty() ? outputString : null);
		} 
		catch (InvocationTargetException e)
		{
			return new CodeTestResult(
				testName, 
				false /*succeeded*/,
				getExceptionText(e.getTargetException(), className, lineNumberOffset),
				null /*returnValue*/,
				null /*output*/);
		}
		catch (IllegalAccessException | IllegalArgumentException e)
		{
			throw new RuntimeException(e);
		}
		finally
		{
			System.setOut(oldOutStream);
		}
	}

	/**
	 * @param returnValue The value to return as a string.
	 * @return The string representation of the value.
	 */
	private String getReturnValueAsString(Object returnValue)
	{
		if (returnValue == null)
			return null;
		else if (returnValue instanceof Float)
			return getNumericValueAsString((Float)returnValue);
		else if (returnValue instanceof Double)
			return getNumericValueAsString((Double)returnValue);
		else if (returnValue instanceof boolean[])
			return Arrays.toString((boolean[])returnValue);
		else if (returnValue instanceof byte[])
			return Arrays.toString((byte[])returnValue);
		else if (returnValue instanceof short[])
			return Arrays.toString((short[])returnValue);
		else if (returnValue instanceof int[])
			return Arrays.toString((int[])returnValue);
		else if (returnValue instanceof long[])
			return Arrays.toString((long[])returnValue);
		else if (returnValue instanceof char[])
			return Arrays.toString((char[])returnValue);
		else if (returnValue instanceof float[])
			return Arrays.toString((float[])returnValue);
		else if (returnValue instanceof double[])
			return Arrays.toString((double[])returnValue);
		else if (returnValue instanceof Object[])
			return Arrays.deepToString((Object[])returnValue);
		else
			return returnValue.toString();
	}
	
	/**
	 * Returns a numeric value, as a string.
	 */
	private String getNumericValueAsString(Number value)
	{
		return new DecimalFormat("0.0#####").format(value);
	}
	
	/**
	 * Returns the exception text for an exception thrown by a test.
	 * @param exception The exception.
	 * @param className The name of the class containing the code being tested.
	 * @param lineNumberOffset The offset to add to the line number, for stack traces.
	 * @return Text describing the exception and the stack trace,
	 * with adjusted line numbers.
	 */
	private String getExceptionText(Throwable exception, String className, int lineNumberOffset)
	{
		String classFileName = String.format("%s.java", className);
		StringBuilder sb = new StringBuilder();
		
		sb.append(String.format("%s\n", exception.toString()));
		
		int lastFrameInClass = -1;
		StackTraceElement[] frames = exception.getStackTrace();
		for (int frameIndex = 0; frameIndex < frames.length; frameIndex++)
		{
			if (frames[frameIndex].getFileName().equals(classFileName))
			{
				lastFrameInClass = frameIndex;
			}
		}
		
		for (int frameIndex = 0; frameIndex <= lastFrameInClass; frameIndex++)
		{
			StackTraceElement frame = frames[frameIndex];
			int lineNumber = frame.getLineNumber() + lineNumberOffset;
			String frameLocation;
			if (frames[frameIndex].getFileName().equals(classFileName))
			{
				if (includeClassNameInStackTraces())
				{
					frameLocation = String.format("%s.%s (Line %d)",
						frame.getClassName(),
						frame.getMethodName(),
						lineNumber);
				}
				else
				{
					frameLocation = String.format("%s (Line %d)",
						frame.getMethodName(),
						lineNumber);
				}
			}
			else
			{
				frameLocation = String.format("%s.%s",
					frame.getClassName(),
					frame.getMethodName());
			}
			
			String frameText = String.format(
				"\tat %s%s",
				frameLocation, 
				frameIndex < lastFrameInClass ? "\n" : "");
			
			sb.append(frameText);
		}
		
		return sb.toString();
	}
}