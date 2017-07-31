using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// Represents a java constructor invocation.
	/// </summary>
	public class JavaConstructorInvocationGenerator : IJavaConstructorInvocationGenerator
	{
		/// <summary>
		/// The java class.
		/// </summary>
		private readonly JavaFileBuilder _builder;

		/// <summary>
		/// The java classes in the object model.
		/// </summary>
		private readonly IList<JavaClass> _javaClasses;

		/// <summary>
		/// Constructor.
		/// </summary>
		public JavaConstructorInvocationGenerator(
			JavaFileBuilder builder, 
			IList<JavaClass> javaClasses)
		{
			_builder = builder;
			_javaClasses = javaClasses;
		}

		/// <summary>
		/// Generates the serializable class.
		/// </summary>
		public void GenerateConstructorInvocation(object obj, string prefix, string suffix)
		{
			var javaClass = _javaClasses.Single(c => c.Type == obj.GetType());

			_builder.AddLine($"{prefix}new {javaClass.ClassName}");
			_builder.BeginScope("(");
				AddParameterValues(javaClass, obj);
			_builder.EndScope($"){suffix}");
		}

		/// <summary>
		/// Returns a list of all properties in the inheritance hierarchy.
		/// </summary>
		private IEnumerable<JavaClassProperty> GetAllProperties(JavaClass javaClass)
		{
			if (javaClass.BaseClass != null)
			{
				foreach (var prop in GetAllProperties(javaClass.BaseClass))
				{
					yield return prop;
				}
			}

			foreach (var prop in javaClass.Properties)
			{
				yield return prop;
			}
		}

		/// <summary>
		/// Adds all parameter values for the constructor invocation.
		/// </summary>
		private void AddParameterValues(JavaClass javaClass, object obj)
		{
			var allProps = GetAllProperties(javaClass).ToList();
			var lastProp = allProps[allProps.Count - 1];

			foreach (var prop in allProps)
			{
				AddParameterValue(javaClass, obj, prop, last: prop == lastProp);
			}
		}

		/// <summary>
		/// Adds a parameter value.
		/// </summary>
		private void AddParameterValue(JavaClass javaClass, object obj, JavaClassProperty prop, bool last)
		{
			var separator = last ? "" : ",";
			var rawValue = javaClass.Type.GetProperty(prop.Name).GetValue(obj);

			if (prop.IsCollection)
			{
				AddCollectionParameterValue(prop, rawValue, separator);
			}
			else if (prop.JavaClass != null)
			{
				GenerateConstructorInvocation(rawValue, prefix: $"/* {prop.Name}: */ ", suffix: separator);
			}
			else if (prop.JavaType == "String")
			{
				AddStringParameterValue(prop, rawValue, separator);
			}
			else if (prop.JavaType == "int")
			{
				AddPrimitiveParameterValue(prop, rawValue.ToString(), separator);
			}
			else if (prop.JavaType == "boolean")
			{
				AddPrimitiveParameterValue(prop, rawValue.Equals(true) ? "true" : "false", separator);
			}
			else
			{
				throw new InvalidOperationException("Unsupported property type.");
			}
		}

		/// <summary>
		/// Adds a parameter value for a string.
		/// </summary>
		/// <param name="prop"></param>
		/// <param name="rawValue"></param>
		/// <param name="separator"></param>
		private void AddStringParameterValue(JavaClassProperty prop, object rawValue, string separator)
		{
			var lines = ((string) rawValue)?.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
			if (lines != null && lines.Length > 1)
			{
				for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
				{
					var line = lines[lineIndex];

					if (lineIndex == 0)
						_builder.AddLine($"/* {prop.Name}: */ {EscapeLine(line, newLine: true)}");
					else if (lineIndex < lines.Length - 1)
						_builder.AddLine($"\t+ {EscapeLine(line, newLine: true)}");
					else
						_builder.AddLine($"\t+ {EscapeLine(line, newLine: false)}{separator}");
				}
			}
			else if (lines != null)
			{
				_builder.AddLine($"/* {prop.Name}: */ {EscapeLine((string)rawValue, newLine: false)}{separator}");
			}
			else
			{
				_builder.AddLine($"/* {prop.Name}: */ null{separator}");
			}
		}

		/// <summary>
		/// Escapes quotes from a line.
		/// </summary>
		private static string EscapeLine(string line, bool newLine)
		{
			return $"\"{line.Replace("\"", "\\\"")}{(newLine ? "\\n" : "")}\"";
		}

		/// <summary>
		/// Adds a primitive parameter value.
		/// </summary>
		private void AddPrimitiveParameterValue(
			JavaClassProperty prop,
			string paramValue,
			string separator)
		{
			_builder.AddLine($"/* {prop.Name}: */ {paramValue}{separator}");
		}

		/// <summary>
		/// Adds a parameter value for a collection property.
		/// </summary>
		private void AddCollectionParameterValue(JavaClassProperty prop, object rawValue, string collectionSeparator)
		{
			var items = ((IEnumerable) (rawValue ?? Enumerable.Empty<object>())).Cast<object>().ToList();
			var lastItem = items.Count > 0 ? items[items.Count - 1] : null;

			_builder.AddLine($"/* {prop.Name}: */ Arrays.asList(new {prop.JavaClass.ClassName}[]");
			_builder.BeginScope("{");
			foreach (var item in items)
			{
				var itemSeparator = item == lastItem ? "" : ",";

				GenerateConstructorInvocation(item, prefix: string.Empty, suffix: itemSeparator);
			}
			_builder.EndScope($"}}){collectionSeparator}");
		}
	}
}

