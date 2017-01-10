package csc.coderunner;

import java.util.List;
import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * A job that compiles a class, and runs tests for that class.
 */
public class ClassJob extends CodeJob<ClassJobResult>
{
	private String fileContents;
	private int lineNumberOffset;
	private List<ClassTest> tests;
	
	/**
	 * Constructor. 
	 * @param className The name of the public class in the file.
	 * @param classesToImport The list of classes to import (possibly including wildcards).
	 * @param fileContents The contents of the java file.
	 * @param lineNumberOffset The offset to apply to line numbers. 
	 * @param tests The tests to run for this class.
	 */
	public ClassJob(
		@JsonProperty("className") String className,
		@JsonProperty("classesToImport") List<String> classesToImport,
		@JsonProperty("fileContents") String fileContents,
		@JsonProperty("lineNumberOffset") int lineNumberOffset,
		@JsonProperty("tests") List<ClassTest> tests)
	{
		super(className, classesToImport);
				
		JavaFileBuilder builder = JavaFileBuilder.createBuilder(classesToImport);
		
		this.lineNumberOffset = lineNumberOffset - builder.getNumLinesAdded();
		
		this.fileContents = builder
			.addLines(fileContents)
			.getFileContents();
		
		this.tests = tests;
	}
	
	/**
	 * @return The contents of the java file.
	 */
	protected String getFileContents()
	{
		return fileContents;
	}
	
	/**
	 * @return The offset to apply to line numbers.
	 */
	protected int getLineNumberOffset()
	{
		return lineNumberOffset;
	}

	/**
	 * @return The tests for this job.
	 */
	@Override
	protected Iterable<? extends CodeTest> getTests()
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
		// For a class job, any class is valid to test.
		
		return true;
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
	public ClassJobResult createResult(
		CompilationResult classCompilationResult,
		CompilationResult testsCompilationResult,
		List<CodeTestResult> codeTestResults,
		ClassDefinition classDefinition)
	{
		return new ClassJobResult(
			classCompilationResult,
			testsCompilationResult,
			codeTestResults,
			classDefinition);
	}
}
