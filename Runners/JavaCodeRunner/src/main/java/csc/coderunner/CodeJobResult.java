package csc.coderunner;

import java.util.List;

/**
 * The result of a job.
 */
public abstract class CodeJobResult
{
	private CompilationResult classCompilationResult;
	private CompilationResult testsCompilationResult;
	private List<CodeTestResult> testResults;
	
	/**
	 * Constructor.
	 * @param classCompileResult The compilation result of the class.
	 * @param testsCompileResult The compilation result of the tests (if the class compiled successfully).
	 * @param testResults The test results (if the tests compiled successfully).
	 */
	public CodeJobResult(
		CompilationResult classCompileResult, 
		CompilationResult testsCompileResult, 
		List<CodeTestResult> testResults)
	{
		this.classCompilationResult = classCompileResult;
		this.testsCompilationResult = testsCompileResult;
		this.testResults = testResults;
	}
	
	/**
	 * @return The compilation result of the class.
	 */
	public CompilationResult getClassCompilationResult()
	{
		return classCompilationResult;
	}

	/**
	 * @return The compilation result of the tests (if the class compiled successfully).
	 */
	public CompilationResult getTestsCompilationResult()
	{
		return testsCompilationResult;
	}
	
	/**
	 * @return The test results (if the tests compiled successfully).
	 */
	public List<CodeTestResult> getTestResults()
	{
		return testResults;
	}
}
