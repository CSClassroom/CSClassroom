package csc.coderunner;

import java.util.List;

public class CompilationResult
{
	private boolean success;
	private List<CompileError> errors;

	public CompilationResult(boolean success, List<CompileError> errors)
	{
		this.success = success;
		
		if (!success)
			this.errors = errors;
	}
	
	/**
	 * @return Whether or not the compilation was successful.
	 */
	public boolean getSuccess()
	{
		return success;
	}

	/**
	 * @return The compiler errors, if the compilation was not successful.
	 */
	public List<CompileError> getErrors()
	{
		return errors;
	}
}
