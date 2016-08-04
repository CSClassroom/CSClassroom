package csc.coderunner;

import java.util.ArrayList;
import java.util.List;

import javax.tools.Diagnostic;
import javax.tools.Diagnostic.Kind;
import javax.tools.DiagnosticListener;
import javax.tools.JavaFileObject;

public class ErrorListener implements DiagnosticListener<JavaFileObject>
{
	private int lineNumberOffset;
	private List<CompileError> compilerErrors = new ArrayList<CompileError>();
	
	/**
	 * Constructor.
	 * @param lineNumberOffset The offset for line numbers.
	 */
	public ErrorListener(int lineNumberOffset)
	{
		this.lineNumberOffset = lineNumberOffset;
	}
	
	/**
	 * @return The compiler errors encountered.
	 */
	public List<CompileError> getCompilerErrors()
	{
		return compilerErrors;
	}
	
	/**
	 * Callback that is called when the compiler wishes to report a diagnostic.
	 */
	@Override
	public void report(Diagnostic<? extends JavaFileObject> diagnostic) 
	{
		if (diagnostic.getKind() == Kind.ERROR)
		{
			compilerErrors.add(new CompileError(diagnostic, lineNumberOffset));
		}
	}
}
