package csc.coderunner;

import javax.tools.Diagnostic;
import javax.tools.JavaFileObject;

/**
 * An error encountered during compilation.
 */
public class CompileError
{
	private int lineNumber;
	private int columnNumber;
	private String message;
	private String fullError;
	
	/**
	 * Constructor.
	 * @param diagnostic The compiler diagnostic.
	 * @param lineNumberOffset The offset to add to the line number.
	 */
	public CompileError(Diagnostic<? extends JavaFileObject> diagnostic, int lineNumberOffset)
	{
		this.lineNumber = (int)(diagnostic.getLineNumber() + lineNumberOffset);
		this.columnNumber = (int)diagnostic.getColumnNumber();
		this.message = diagnostic.getMessage(null);
		
		String rawError = diagnostic.toString();
		String errorPrefix = "error: ";
		int rawErrorBegin = rawError.indexOf(errorPrefix);
		if (rawErrorBegin != -1)
		{
			fullError = String.format("Error on line %d: %s",
				lineNumber,
				rawError.substring(rawErrorBegin + errorPrefix.length()));
		}
		else
		{
			fullError = rawError;
		}
	}
	
	/**
	 * @return The line number of the error.
	 */
	public long getLineNumber()
	{
		return lineNumber;
	}

	/**
	 * @return The column number of the error.
	 */
	public long getColumnNumber()
	{
		return columnNumber;
	}

	/**
	 * @return The error message.
	 */
	public String getMessage()
	{
		return message;
	}

	/**
	 * @return The full error message, including context.
	 */
	public String getFullError()
	{
		return fullError;
	}
}
