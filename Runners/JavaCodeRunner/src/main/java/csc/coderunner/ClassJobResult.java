package csc.coderunner;

import java.util.List;

/**
 * The result of a class job.
 */
public class ClassJobResult extends CodeJobResult
{
	private ClassDefinition classDefinition;
	
	/**
	 * Constructor.
	 * @param classCompileResult The compilation result of the class.
	 * @param testsCompileResult The compilation result of the tests (if the class compiled successfully).
	 * @param testResults The test results (if the tests compiled successfully).
	 * @param classDefinition The definition of the class (if the class compiled successfully).
	 */
	public ClassJobResult(
		CompilationResult classCompileResult, 
		CompilationResult testsCompileResult, 
		List<CodeTestResult> testResults,
		ClassDefinition classDefinition)
	{
		super(classCompileResult, testsCompileResult, testResults);
		this.classDefinition = classDefinition;
	}
	
	/**
	 * @return The definition of the class (if the class compiled successfully).
	 */
	public ClassDefinition getClassDefinition()
	{
		return classDefinition;
	}
}
