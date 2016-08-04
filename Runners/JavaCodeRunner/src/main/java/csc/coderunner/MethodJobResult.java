package csc.coderunner;

import java.util.List;

/**
 * The result of a method job.
 */
public class MethodJobResult extends CodeJobResult
{
	private MethodDefinition methodDefinition;
	
	/**
	 * Constructor.
	 * @param classCompileResult The compilation result of the class.
	 * @param testsCompileResult The compilation result of the tests (if the class compiled successfully).
	 * @param testResults The test results (if the tests compiled successfully).
	 * @param classDefinition The definition of the method (if the method compiled successfully).
	 */
	public MethodJobResult(
		CompilationResult classCompileResult, 
		CompilationResult testsCompileResult, 
		List<CodeTestResult> testResults,
		MethodDefinition methodDefinition)
	{
		super(classCompileResult, testsCompileResult, testResults);
		this.methodDefinition = methodDefinition;
	}
	
	/**
	 * @return The definition of the class (if the class compiled successfully).
	 */
	public MethodDefinition getMethodDefinition()
	{
		return methodDefinition;
	}
}
