package csc.coderunner;

import static org.junit.Assert.*;
import static csc.coderunner.CodeJobTestAsserts.*;
import java.util.Arrays;
import org.junit.Test;

public class MethodJobTest
{
	@Test
	public void testFailedMethodCompilation()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public bogus int fail(int a, int b)"
				+ "\n" + "{"
				+ "\n" + "	return bogus value"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "3, 4")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationFailed(result.getClassCompilationResult(), new int[] { 1, 3, 3, 3 });
		assertNull(result.getMethodDefinition());
		assertNull(result.getTestsCompilationResult());
		assertNull(result.getTestResults());
	}
	
	@Test
	public void testTooManyMethods()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static int addIntegers(int a, int b)"
				+ "\n" + "{"
				+ "\n" + "	return a + b;"
				+ "\n" + "}"
				
		+	"public static int subtractIntegers(int a, int b)"
				+ "\n" + "{"
				+ "\n" + "	return a - b;"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "3, 4")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		assertNull(result.getMethodDefinition());
		assertNull(result.getTestsCompilationResult());
		assertNull(result.getTestResults());
	}
	
	@Test
	public void testFailedTestsCompilation()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static void failTests(int a, int b)"
				+ "\n" +"{"
				+ "\n" +"	return;"
				+ "\n" +"}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "3, 4, 5")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"failTests",
			true /*isStatic*/,
			new String[] { "int", "int" },
			"void");
		
		assertCompilationFailed(result.getTestsCompilationResult(), null);
		
		assertNull(result.getTestResults());
	}
	
	@Test
	public void testExceptionDuringTest()
	{
		MethodJob job = new MethodJob(
			Arrays.asList("java.util.*"),
			
			"public static String getFourthElement(List<String> list)"
				+ "\n" +"{"
				+ "\n" +"	return list.get(4);"
				+ "\n" +"}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "Arrays.asList(\"1\", \"2\", \"3\")")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"getFourthElement",
			true /*isStatic*/,
			new String[] { "List" },
			"String");

		assertCompilationSucceeded(result.getTestsCompilationResult());
		
		assertTestResults(
			true /*output*/, 
			false /*expectSuccess*/,
			new String[]
			{ 
				  "java.lang.ArrayIndexOutOfBoundsException: Index 4 out of bounds for length 3\n"
				+ "\tat java.util.Arrays$ArrayList.get\n"
				+ "\tat getFourthElement (Line 3)" 
			}, 
			result.getTestResults());
	}	
	
	@Test
	public void voidReturnType()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static void printIntegers(int a, int b)"
				+ "\n" + "{"
				+ "\n" + "	System.out.print(a + \" \" + b);"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "3, 4"),
				new MethodTest("Test2", "-2, 6")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"printIntegers",
			true /*isStatic*/,
			new String[] { "int", "int" },
			"void");
		
		assertTestResults(
			true /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "3 4", "-2 6" }, 
			result.getTestResults());
	}
	
	@Test
	public void intReturnType()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static int addIntegers(int a, int b)"
				+ "\n" + "{"
				+ "\n" + "	return a + b;"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "3, 4"),
				new MethodTest("Test2", "-2, 6")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"addIntegers",
			true /*isStatic*/,
			new String[] { "int", "int" },
			"int");
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "7", "4" }, 
			result.getTestResults());
	}
	
	@Test
	public void doubleReturnType()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static double getValue()"
				+ "\n" + "{"
				+ "\n" + "	return 1.5000000001;"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"getValue",
			true /*isStatic*/,
			new String[] { },
			"double");
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "1.5" }, 
			result.getTestResults());
	}
	
	@Test
	public void primitiveArrayReturnType()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static int[] createArray(int a, int b)"
				+ "\n" + "{"
				+ "\n" + "	return new int[] { a, b };"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "3, 4"),
				new MethodTest("Test2", "-2, 6")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"createArray",
			true /*isStatic*/,
			new String[] { "int", "int" },
			"int[]");
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "[3, 4]", "[-2, 6]" }, 
			result.getTestResults());
	}
	
	@Test
	public void objectArrayReturnType()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static String[] createArray(String a, String b)"
				+ "\n" + "{"
				+ "\n" + "	return new String[] { a, b };"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "\"3\", \"4\""),
				new MethodTest("Test2", "\"-2\", \"6\"")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"createArray",
			true /*isStatic*/,
			new String[] { "String", "String" },
			"String[]");
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "[3, 4]", "[-2, 6]" }, 
			result.getTestResults());
	}
	
	@Test
	public void twoDimensionalArrayReturnType()
	{
		MethodJob job = new MethodJob(
			Arrays.asList() /*classesToImport*/,
			
			"public static int[][] createArray(int rows, int cols)"
				+ "\n" + "{"
				+ "\n" + "	int[][] arr = new int[rows][cols];"
				+ "\n" + ""
				+ "\n" + "	for (int i = 0; i < rows; i++)"
				+ "\n" + "	{"
				+ "\n" + "		for (int j = 0; j < cols; j++)"
				+ "\n" + "		{"
				+ "\n" + "			arr[i][j] = i*cols + j;"
				+ "\n" + "		}"
				+ "\n" + "	}"
				+ "\n" + ""
				+ "\n" + "	return arr;"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "2, 3")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"createArray",
			true /*isStatic*/,
			new String[] { "int", "int" },
			"int[][]");
		
		assertTestResults(
			false /*output*/,
			true /*expectSuccess*/,
			new String[] { "[[0, 1, 2], [3, 4, 5]]" },
			result.getTestResults());
	}
	
	@Test
	public void objectReturnType()
	{
		MethodJob job = new MethodJob(
			Arrays.asList("java.util.*"),
			
			"public static List<String> createList(String a, String b)"
				+ "\n" + "{"
				+ "\n" + "	List<String> list = new ArrayList<String>();"
				+ "\n" + ""
				+ "\n" + "	list.add(a);"
				+ "\n" + "	list.add(b);"
				+ "\n" + ""
				+ "\n" + "	return list;"
				+ "\n" + "}",
			
			Arrays.asList
			(
				new MethodTest("Test1", "\"a\", \"b\""),
				new MethodTest("Test2", "null, \"notnull\"")
			));
		
		MethodJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertMethodEquals(
			result.getMethodDefinition(),
			"createList",
			true /*isStatic*/,
			new String[] { "String", "String" },
			"List");
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "[a, b]", "[null, notnull]" }, 
			result.getTestResults());
	}
}
