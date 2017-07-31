using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// Generates a java file containing a single class.
	/// </summary>
	public class JavaFileBuilder
	{
		/// <summary>
		/// The number of lines added.
		/// </summary>
		public int LinesAdded { get; private set; }

		/// <summary>
		/// The string builder that contains the contents of the file to generate.
		/// </summary>
		private readonly StringBuilder _sb = new StringBuilder();

		/// <summary>
		/// The current indent level.
		/// </summary>
		private int _indentLevel;

		/// <summary>
		/// Outputs a { character, and increases the indentation level.
		/// </summary>
		public JavaFileBuilder BeginScope(string scopeOpen)
		{
			AddLine(scopeOpen);
			_indentLevel++;

			return this;
		}

		/// <summary>
		/// Decreases the indentation level, and outputs a } character.
		/// </summary>
		public JavaFileBuilder EndScope(string scopeClose)
		{
			_indentLevel--;
			AddLine(scopeClose);

			return this;
		}

		/// <summary>
		/// Adds a line, indented by the current indentation level.
		/// </summary>
		public JavaFileBuilder AddLine(string line)
		{
			for (int i = 0; i < _indentLevel; i++)
			{
				_sb.Append("\t");
			}

			_sb.Append(line);
			_sb.Append("\n");
			LinesAdded++;

			return this;
		}

		/// <summary>
		/// Adds a blank line.
		/// </summary>
		public JavaFileBuilder AddBlankLine()
		{
			_sb.Append("\n");
			LinesAdded++;

			return this;
		}

		/// <summary>
		/// Adds each line in the given string, indented by the current 
		/// indentation level.
		/// </summary>
		public JavaFileBuilder AddLines(string lines)
		{
			return AddLines(lines.Split('\n'));
		}

		/// <summary>
		/// Adds each line in the given string, indented by the current 
		/// indentation level.
		/// </summary>
		public JavaFileBuilder AddLines(IEnumerable<string> lines)
		{
			foreach (string line in lines)
			{
				AddLine(line);
			}

			return this;
		}

		/// <summary>
		/// Returns the full contents of the file.
		/// </summary>
		public string GetFileContents()
		{
			return _sb.ToString();
		}
	}
}
