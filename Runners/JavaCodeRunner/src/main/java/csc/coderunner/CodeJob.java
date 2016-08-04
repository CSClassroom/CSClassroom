package csc.coderunner;

import java.util.ArrayList;
import java.util.List;

import net.openhft.compiler.CompilerUtils;

/**
 * A job that compiles and tests code.
 */
public abstract class CodeJob<TResult extends CodeJobResult>
{
	private String className;
	private List<String> classesToImport;
	
	/**
	 * Constructor. 
	 * @param className The name of the public class in the file.
	 * @param classesToImport The list of classes to import (possibly including wildcards).
	 */
	public CodeJob(
		String className,
		List<String> classesToImport)
	{
		this.className = className;
		this.classesToImport = classesToImport;
	}
	
	/**
	 * @return The contents of the java file.
	 */
	protected abstract String getFileContents();
	
	/**
	 * @return The offset to apply to line numbers.
	 */
	protected abstract int getLineNumberOffset();
	
	/**
	 * @return The tests for this job.
	 */
	protected abstract Iterable<? extends CodeTest> getTests();
	
	/**
	 * Validates that the compiled class is valid.
	 * @param javaClass The compiled class.
	 * @return Whether or not the compiled class is valid.
	 */
	protected abstract boolean validateClass(Class<?> javaClass);
	
	/**
	 * Creates the result of the job.
	 * @param classCompilationResult The class compilation result.
	 * @param testsCompilationResult The tests compilation result.
	 * @param codeTestResults The test results.
	 * @param classDefinition The class definition.
	 * @return The job result.
	 */
	protected abstract TResult createResult(
		CompilationResult classCompilationResult,
		CompilationResult testsCompilationResult,
		List<CodeTestResult> codeTestResults,
		ClassDefinition classDefinition);
	
	/**
	 * @return The class name.
	 */
	protected String getClassName()
	{
		return className;
	}
	
	/**
	 * @return The list of classes to import (possibly including wildcards).
	 */
	protected List<String> getClassesToImport()
	{
		return classesToImport;
	}
	
	/***
	 * Runs the job, which does the following:
	 *  - Compiles the class
	 *  - Generates the class definition
	 *  - Compiles the tests
	 *  - Runs the tests
	 *  - Returns the results
	 * @return The results of the job.
	 */
	public TResult runJob()
	{
		CompiledClass compiledClass = compile(className, getFileContents(), getLineNumberOffset());
		if (!compiledClass.compilationSucceeded() || !validateClass(compiledClass.getJavaClass()))
		{
			return createResult(compiledClass.getCompilationResult(), null, null, null);
		}

		ClassDefinition definition = new ClassDefinition(compiledClass.getJavaClass());
		
		CompiledClass compiledTests = compile(
			getTestsClassName(), 
			getTestsFileContents(compiledClass.getJavaClass()), 
			0 /*lineOffset*/);
		
		if (!compiledTests.compilationSucceeded())
		{
			return createResult(
				compiledClass.getCompilationResult(), 
				compiledTests.getCompilationResult(), 
				null /*testResults*/,
				definition);
		}
		
		List<CodeTestResult> testResults = getTestResults(compiledTests.getJavaClass());
		
		return createResult(
			compiledClass.getCompilationResult(),
			compiledTests.getCompilationResult(), 
			testResults,
			definition);
	}
	
	/**
	 * @return The name of the test class.
	 */
	private String getTestsClassName()
	{
		return className + "Tests";
	}
	
	/**
	 * @param javaClass The class containing the code that will be tested.
	 * @return The contents of the generated tests file.
	 */
	private String getTestsFileContents(Class<?> javaClass)
	{
		JavaFileBuilder fileBuilder = JavaFileBuilder.createBuilder(classesToImport)
			.addLine("public class %s", getTestsClassName())
			.beginScope();
		
		for (CodeTest test : getTests())
		{
			test.addTestMethod(fileBuilder, javaClass);
		}
		
		fileBuilder.endScope();
		
		return fileBuilder.getFileContents();
	}
	
	/**
	 * @param testsClass The compiled tests class.
	 * @return The results of running the tests.
	 */
	private List<CodeTestResult> getTestResults(Class<?> testsClass)
	{
		List<CodeTestResult> testResults = new ArrayList<CodeTestResult>();
		
		for (CodeTest test : getTests())
		{
			testResults.add(test.runTest(className, testsClass, getLineNumberOffset()));
		}
		
		return testResults;
	}
	
	/**
	 * Compiles a java file.
	 * @param className The name of the class.
	 * @param javaFileContents The contents of the java file.
	 * @return The compiled class.
	 */
	private static CompiledClass compile(String className, String javaFileContents, int lineNumberOffset)
	{
		ErrorListener errorListener = new ErrorListener(lineNumberOffset);
		Class<?> javaClass = null;
		
		try
		{
			javaClass = CompilerUtils.CACHED_COMPILER.loadFromJava(
				className, 
				javaFileContents, 
				errorListener);
		}
		catch (ClassNotFoundException ex)
		{
		}
		
		CompilationResult compilationResult = new CompilationResult(
			javaClass != null, 
			errorListener.getCompilerErrors());
		
		return new CompiledClass(compilationResult, javaClass);
	}
}