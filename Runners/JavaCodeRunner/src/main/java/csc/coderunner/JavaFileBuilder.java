package csc.coderunner;

import java.util.List;

/**
 * Generates a java file containing a single class.
 */
public class JavaFileBuilder
{
	private StringBuilder sb = new StringBuilder();
	private int indentLevel = 0;
	private int linesAdded = 0;
	
	/**
	 * Creates a builder.
	 * @param classesToImport The classes to import.
	 * @return The builder.
	 */
	public static JavaFileBuilder createBuilder(List<String> classesToImport)
	{
		JavaFileBuilder builder = new JavaFileBuilder();
		
		for (String classToImport : classesToImport)
		{
			builder.addImport(classToImport);
		}
		
		return builder;
	}
	
	/**
	 * Adds an import to the java file.
	 * @param classToImport The classes to import.
	 * @return The builder.
	 */
	private JavaFileBuilder addImport(String classToImport)
	{
		sb.append(String.format("import %s;\n", classToImport));
		linesAdded++;
		
		return this;
	}
	
	/**
	 * @return Returns the number of lines added so far.
	 */
	public int getNumLinesAdded()
	{
		return linesAdded;
	}
	
	/**
	 * Outputs a { character, and increases the indentation level.
	 * @return The builder.
	 */
	public JavaFileBuilder beginScope()
	{
		addLine("{");
		indentLevel++;
		
		return this;
	}
	
	/**
	 * Decreases the indentation level.
	 * @return The builder.
	 */
	public JavaFileBuilder endScope()
	{
		indentLevel--;
		addLine("}");
		
		return this;
	}
	
	/**
	 * Adds a line, indented by the current indentation level.
	 * @param line The line to add.
	 * @return The builder.
	 */
	public JavaFileBuilder addLine(String line, Object... args)
	{
		for (int i = 0; i < indentLevel; i++)
		{
			sb.append("\t");
		}
		
		sb.append(String.format(line, args));
		sb.append("\n");
		linesAdded++;
		
		return this;
	}
	
	/**
	 * Adds a blank line (not indented).
	 * @return The builder.
	 */
	public JavaFileBuilder addBlankLine()
	{
		sb.append("\n");
		linesAdded++;
		
		return this;
	}
	
	/**
	 * Adds each line in the given string, indented by the current 
	 * indentation level.
	 * @param lines The string containing the lines to add.
	 * @return The builder.
	 */
	public JavaFileBuilder addLines(String lines)
	{
		for (String line : lines.split("\\r?\\n"))
		{
			addLine(line);
		}
		
		return this;
	}
	
	/**
	 * @return The full contents of the file.
	 */
	public String getFileContents()
	{
		return sb.toString();
	}
}
