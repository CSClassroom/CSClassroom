package csc.coderunner;

public class CompiledClass
{
	private CompilationResult result;
	private Class<?> javaClass;
	
	public CompiledClass(CompilationResult result, Class<?> javaClass)
	{
		this.result = result;
		this.javaClass = javaClass;
	}
	
	/**
	 * @return Whether or not the compilation succeeded.
	 */
	public boolean compilationSucceeded()
	{
		return result.getSuccess();
	}
	
	/**
	 * @return The compilation result.
	 */
	public CompilationResult getCompilationResult()
	{
		return result;
	}
	
	/**
	 * @return The Java class object for the class, if it successfully compiled.
	 */
	public Class<?> getJavaClass()
	{
		return javaClass;
	}
}
