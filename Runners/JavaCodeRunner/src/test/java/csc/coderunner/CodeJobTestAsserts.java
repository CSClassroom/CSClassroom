package csc.coderunner;

import static org.junit.Assert.*;

import java.util.List;

public class CodeJobTestAsserts
{
	public static void assertCompilationSucceeded(CompilationResult compilationResult)
	{
		assertNotNull(compilationResult);
		assertTrue(compilationResult.getSuccess());
	}
	
	public static void assertCompilationFailed(CompilationResult compilationResult, int[] lineNumbers)
	{
		assertNotNull(compilationResult);
		assertFalse(compilationResult.getSuccess());
		
		if (lineNumbers != null)
		{
			assertEquals(lineNumbers.length, compilationResult.getErrors().size());
			for (int errorNumber = 0; errorNumber < lineNumbers.length; errorNumber++)
			{
				assertEquals(
					lineNumbers[errorNumber],
					compilationResult.getErrors().get(errorNumber).getLineNumber());
			}
		}
		else
		{
			assertFalse(compilationResult.getErrors().isEmpty());
		}
	}
	
	public static void assertSingleStaticMethod(
		ClassDefinition definition,
		String expectedClassName,
		String expectedMethodName, 
		String[] expectedParamTypes, 
		String expectedReturnType)
	{
		assertEquals(expectedClassName, definition.getName());
		
		assertEquals(0, definition.getFields().size());
		
		assertEquals(2, definition.getMethods().size());
		
		assertMethodEquals(
			definition.getMethods().get(0), 
			expectedClassName, 
			false /*expectedIsStatic*/, 
			new String[] { }, 
			null /*expectedReturnType*/);
		
		assertMethodEquals(
			definition.getMethods().get(1), 
			expectedMethodName, 
			true /*expectedIsStatic*/, 
			expectedParamTypes, 
			expectedReturnType);
	}
	
	public static void assertMethodEquals(
		MethodDefinition methodDefinition,
		String expectedMethodName,
		boolean expectedIsStatic,
		String[] expectedParamTypes,
		String expectedReturnType)
	{
		assertEquals(expectedMethodName, methodDefinition.getName());
		
		assertEquals(expectedIsStatic, methodDefinition.getIsStatic());
		assertTrue(methodDefinition.getIsPublic());
		
		assertEquals(expectedParamTypes.length, methodDefinition.getParameterTypes().size());
		for (int paramTypeIndex = 0; paramTypeIndex < expectedParamTypes.length; paramTypeIndex++)
		{
			assertEquals(
				expectedParamTypes[paramTypeIndex],
				methodDefinition.getParameterTypes().get(paramTypeIndex));
		}
		
		assertEquals(expectedReturnType, methodDefinition.getReturnType());
	}

	public static void assertFieldEquals(
		FieldDefinition fieldDefinition,
		String expectedFieldName,
		String expectedFieldType)
	{
		assertEquals(expectedFieldName, fieldDefinition.getName());
		
		assertFalse(fieldDefinition.getIsStatic());
		assertFalse(fieldDefinition.getIsPublic());
		
		assertEquals(expectedFieldType, fieldDefinition.getType());
	}
	
	public static void assertTestResults(
		boolean outputTest, 
		boolean expectSuccess, 
		String[] expectedTestResults, 
		List<CodeTestResult> testResults)
	{
		assertEquals(expectedTestResults.length, testResults.size());
		
		for (int testResultIndex = 0; testResultIndex < expectedTestResults.length; testResultIndex++)
		{
			CodeTestResult testResult = testResults.get(testResultIndex);
			
			if (expectSuccess)
			{
				assertTrue(testResult.getCompleted());
				
				if (outputTest)
				{
					assertNull(testResult.getReturnValue());
					assertEquals(expectedTestResults[testResultIndex], testResult.getOutput());
				}
				else
				{
					assertEquals(expectedTestResults[testResultIndex], testResult.getReturnValue());
					assertNull(testResult.getOutput());
				}
			}
			else
			{
				assertFalse(testResult.getCompleted());
				assertEquals(expectedTestResults[testResultIndex], testResult.getException());
			}
		}
	}
}
