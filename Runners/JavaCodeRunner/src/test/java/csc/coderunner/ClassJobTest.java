package csc.coderunner;

import static org.junit.Assert.*;
import static csc.coderunner.CodeJobTestAsserts.*;
import java.util.Arrays;
import org.junit.Test;

public class ClassJobTest
{	
	@Test
	public void testFailedClassCompilation()
	{
		ClassJob job = new ClassJob(
			"FailedClassCompilation",
			
			Arrays.asList() /*classesToImport*/,
			
			"public class FailedClassCompilation"
				+ "\n" + 	"{"
				+ "\n" +	"	public bogus int fail(int a, int b)"
				+ "\n" +	"	{"
				+ "\n" +	"		return bogus value"
				+ "\n" +	"	}"
				+ "\n" +	"}",
				
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "int", "return FailedClassCompilation.fail(3, 4);")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationFailed(result.getClassCompilationResult(), new int[] { 1, 1, 3, 3, 3 });
		assertNull(result.getClassDefinition());
		assertNull(result.getTestsCompilationResult());
		assertNull(result.getTestResults());
	}
	
	@Test
	public void testFailedTestsCompilation()
	{
		ClassJob job = new ClassJob(
			"FailedTestsCompilation",
			
			Arrays.asList() /*classesToImport*/,
			
			"public class FailedTestsCompilation"
				+ "\n" + 	"{"
				+ "\n" +	"	public static void failTests(int a, int b)"
				+ "\n" +	"	{"
				+ "\n" +	"		return;"
				+ "\n" +	"	}"
				+ "\n" +	"}",
				
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "void", "FailedTestCompilation.failTests(3, 4, 5);")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertSingleStaticMethod(
			result.getClassDefinition(),
			"FailedTestsCompilation",
			"failTests",
			new String[] { "int", "int" },
			"void");
		
		assertCompilationFailed(result.getTestsCompilationResult(), null);
		
		assertNull(result.getTestResults());
	}
	
	@Test
	public void testExceptionDuringTest()
	{
		ClassJob job = new ClassJob(
			"ExceptionOperation",
			
			Arrays.asList("java.util.*"),
			
			"public class ExceptionOperation"
				+ "\n" + 	"{"
				+ "\n" +	"	public static String getFourthElement(List<String> list)"
				+ "\n" +	"	{"
				+ "\n" +	"		return list.get(4);"
				+ "\n" +	"	}"
				+ "\n" +	"}",
				
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "String", "return ExceptionOperation.getFourthElement(Arrays.asList(\"1\", \"2\", \"3\"));")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertSingleStaticMethod(
			result.getClassDefinition(),
			"ExceptionOperation",
			"getFourthElement",
			new String[] { "List" },
			"String");

		assertCompilationSucceeded(result.getTestsCompilationResult());
		
		assertTestResults(
			true /*output*/, 
			false /*expectSuccess*/,
			new String[] 
			{ 
				  "java.lang.ArrayIndexOutOfBoundsException: 4\n"
				+ "\tat java.util.Arrays$ArrayList.get\n"
				+ "\tat ExceptionOperation.getFourthElement (Line 4)" 
			}, 
			result.getTestResults());
	}	
	
	@Test
	public void primitiveReturnType()
	{
		ClassJob job = new ClassJob(
			"IntegerOperation",
			
			Arrays.asList() /*classesToImport*/,
			
			"public class IntegerOperation"
				+ "\n" + 	"{"
				+ "\n" +	"	public static int addIntegers(int a, int b)"
				+ "\n" +	"	{"
				+ "\n" +	"		return a + b;"
				+ "\n" +	"	}"
				+ "\n" +	"}",
			
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "int", "return IntegerOperation.addIntegers(3, 4);"),
				new ClassTest("Test2", "String", "return \"\" + IntegerOperation.addIntegers(-2, 6);")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertSingleStaticMethod(
			result.getClassDefinition(),
			"IntegerOperation",
			"addIntegers",
			new String[] { "int", "int" },
			"int");
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "7", "4" }, 
			result.getTestResults());
	}
	
	@Test
	public void primitiveArrayReturnType()
	{
		ClassJob job = new ClassJob(
			"PrimitiveArrayOperation",
			
			Arrays.asList() /*classesToImport*/,
			
			"public class PrimitiveArrayOperation"
				+ "\n" + 	"{"
				+ "\n" +	"	public static int[] createArray(int a, int b)"
				+ "\n" +	"	{"
				+ "\n" +	"		return new int[] { a, b };"
				+ "\n" +	"	}"
				+ "\n" +	"}",
			
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "int[]", "return PrimitiveArrayOperation.createArray(3, 4);"),
				new ClassTest("Test2", "int[]", "return PrimitiveArrayOperation.createArray(-2, 6);")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertSingleStaticMethod(
			result.getClassDefinition(),
			"PrimitiveArrayOperation",
			"createArray",
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
		ClassJob job = new ClassJob(
			"ObjectArrayOperation",
			
			Arrays.asList() /*classesToImport*/,
			
			"public class ObjectArrayOperation"
				+ "\n" + 	"{"
				+ "\n" +	"	public static String[] createArray(String a, String b)"
				+ "\n" +	"	{"
				+ "\n" +	"		return new String[] { a, b };"
				+ "\n" +	"	}"
				+ "\n" +	"}",
			
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "String[]", "return ObjectArrayOperation.createArray(\"3\", \"4\");"),
				new ClassTest("Test2", "Object[]", "return ObjectArrayOperation.createArray(\"-2\", \"6\");")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertSingleStaticMethod(
			result.getClassDefinition(),
			"ObjectArrayOperation",
			"createArray",
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
		ClassJob job = new ClassJob(
			"TwoDimensionalArrayOperation",
			
			Arrays.asList() /*classesToImport*/,
			
			"public class TwoDimensionalArrayOperation"
				+ "\n" + 	"{"
				+ "\n" +	"	public static int[][] createArray(int rows, int cols)"
				+ "\n" +	"	{"
				+ "\n" +	"		int[][] arr = new int[rows][cols];"
				+ "\n" +	""
				+ "\n" +	"		for (int i = 0; i < rows; i++)"
				+ "\n" +	"		{"
				+ "\n" +	"			for (int j = 0; j < cols; j++)"
				+ "\n" +	"			{"
				+ "\n" +	"				arr[i][j] = i*cols + j;"
				+ "\n" +	"			}"
				+ "\n" +	"		}"
				+ "\n" +	""
				+ "\n" +	"		return arr;"
				+ "\n" +	"	}"
				+ "\n" +	"}",
			
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "int[][]", "return TwoDimensionalArrayOperation.createArray(2, 3);")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertSingleStaticMethod(
			result.getClassDefinition(),
			"TwoDimensionalArrayOperation",
			"createArray",
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
		ClassJob job = new ClassJob(
			"ObjectOperation",
			
			Arrays.asList("java.util.*"),
			
			"public class ObjectOperation"
				+ "\n" + 	"{"
				+ "\n" +	"	public static List<String> createList(String a, String b)"
				+ "\n" +	"	{"
				+ "\n" +	"		List<String> list = new ArrayList<String>();"
				+ "\n" +	""
				+ "\n" +	"		list.add(a);"
				+ "\n" +	"		list.add(b);"
				+ "\n" +	""
				+ "\n" +	"		return list;"
				+ "\n" +	"	}"
				+ "\n" +	"}",
			
			-2 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "List<String>", "return ObjectOperation.createList(\"a\", \"b\");"),
				new ClassTest("Test2", "List<String>", "return ObjectOperation.createList(null, \"notnull\");")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		assertSingleStaticMethod(
			result.getClassDefinition(),
			"ObjectOperation",
			"createList",
			new String[] { "String", "String" },
			"List");
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "[a, b]", "[null, notnull]" }, 
			result.getTestResults());
	}

	@Test
	public void testMultipleClasses()
	{
		ClassJob job = new ClassJob(
			"Line",
			
			Arrays.asList() /*classesToImport*/,
			
			"public class Line"
				+ "\n" + 	"{"
				+ "\n" + 	"	private Point start;"
				+ "\n" + 	"	private Point end;"
				+ "\n" +	"	public Line(Point start, Point end)"
				+ "\n" +	"	{"
				+ "\n" +	"		this.start = start;"
				+ "\n" +	"		this.end = end;"
				+ "\n" +	"	}"
				+ "\n" +	"	public String toString()"
				+ "\n" +	"	{"
				+ "\n" +	"		return \"[\" + start + \", \" + end + \"]\";"
				+ "\n" +	"	}"
				+ "\n" +	"}"

		+	"class Point"
				+ "\n" + 	"{"
				+ "\n" + 	"	private int x;"
				+ "\n" + 	"	private int y;"
				+ "\n" +	"	public Point(int x, int y)"
				+ "\n" +	"	{"
				+ "\n" +	"		this.x = x;"
				+ "\n" +	"		this.y = y;"
				+ "\n" +	"	}"
				+ "\n" +	"	public String toString()"
				+ "\n" +	"	{"
				+ "\n" +	"		return \"(\" + x + \", \" + y + \")\";"
				+ "\n" +	"	}"
				+ "\n" +	"}",
				
			-10 /*lineOffset*/, 
			
			Arrays.asList
			(
				new ClassTest("Test1", "Line", "return new Line(new Point(0,1), new Point(2,3));")
			));
		
		ClassJobResult result = job.runJob();
		
		assertCompilationSucceeded(result.getClassCompilationResult());
		
		ClassDefinition definition = result.getClassDefinition();
		assertEquals("Line", definition.getName());
		
		assertEquals(2, definition.getFields().size());
		assertFieldEquals(definition.getFields().get(0), "start", "Point");
		assertFieldEquals(definition.getFields().get(1), "end", "Point");
		
		assertEquals(2, definition.getMethods().size());
		assertMethodEquals(
			definition.getMethods().get(0), 
			"Line", 
			false /*expectedIsStatic*/, 
			new String[] { "Point", "Point" }, 
			null /*returnType*/);
		assertMethodEquals(
			definition.getMethods().get(1), 
			"toString", 
			false /*expectedIsStatic*/, 
			new String[] { }, 
			"String" /*returnType*/);

		assertCompilationSucceeded(result.getTestsCompilationResult());
		
		assertTestResults(
			false /*output*/, 
			true /*expectSuccess*/, 
			new String[] { "[(0, 1), (2, 3)]" }, 
			result.getTestResults());
	}
}
