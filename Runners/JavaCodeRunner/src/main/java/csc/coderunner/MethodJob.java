package csc.coderunner;

import java.lang.reflect.Modifier;
import java.util.List;
import java.util.Random;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * A job that compiles a single static method, and runs tests for that method.
 */
public class MethodJob extends CodeJob<MethodJobResult>
{
	private String fileContents;
	private int lineNumberOffset;
	private List<MethodTest> tests;
	private static final String c_classNamePrefix = "MethodJobClass_";
	
	/**
	 * Constructor. 
	 * @param classesToImport The list of classes to import (possibly including wildcards).
	 * @param methodCode The code for the method (including the signature).
	 * @param tests The tests to run for this class.
	 */
	public MethodJob(
		@JsonProperty("classesToImport") List<String> classesToImport,
		@JsonProperty("methodCode") String methodCode,
		@JsonProperty("tests") List<MethodTest> tests)
	{
		super(generateClassName(), classesToImport);
		
		JavaFileBuilder builder = JavaFileBuilder.createBuilder(getClassesToImport())
				.addLine("public class %s", getClassName())
				.beginScope();
		
		this.lineNumberOffset = -builder.getNumLinesAdded();
		
		this.fileContents = builder.addLines(methodCode)
			.endScope()
			.getFileContents();
		
		this.tests = tests;
	}
	
	/**
	 * @return The contents of the java file.
	 */
	public String getFileContents()
	{
		return fileContents;
	}
	
	/**
	 * @return The offset to apply to line numbers.
	 */
	public int getLineNumberOffset()
	{
		return lineNumberOffset;
	}

	/**
	 * @return The tests for this job.
	 */
	@Override
	public Iterable<? extends CodeTest> getTests()
	{
		return tests;
	}

	/**
	 * Validates that the compiled class is valid.
	 * @param javaClass The compiled class.
	 * @return Whether or not the compiled class is valid.
	 */
	protected boolean validateClass(Class<?> javaClass)
	{
		// For a method job, only classes with a single method are considered valid.
		
		return javaClass.getDeclaredMethods().length == 1
			&& Modifier.isStatic(javaClass.getMethods()[0].getModifiers());
	}

	/**
	 * Creates the result of the job.
	 * @param classCompilationResult The class compilation result.
	 * @param testsCompilationResult The tests compilation result.
	 * @param codeTestResults The test results.
	 * @param classDefinition The class definition.
	 * @return The job result.
	 */
	@Override
	public MethodJobResult createResult(
		CompilationResult classCompilationResult,
		CompilationResult testsCompilationResult, 
		List<CodeTestResult> codeTestResults,
		ClassDefinition classDefinition)
	{
		return new MethodJobResult(
			classCompilationResult,
			testsCompilationResult,
			codeTestResults,
			classDefinition != null
				? classDefinition.getMethods().get(1)
				: null);
	}
	
	/**
	 * @return A randomly-generated class name to use.
	 */
	private static String generateClassName()
	{
		return String.format("%s%s",
			c_classNamePrefix, 
			"" + new Random().nextInt(Integer.MAX_VALUE));
	}
}
