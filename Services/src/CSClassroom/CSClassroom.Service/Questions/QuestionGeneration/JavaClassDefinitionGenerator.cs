using System.Collections.Generic;
using System.Linq;
using CSC.Common.Infrastructure.Extensions;

namespace CSC.CSClassroom.Service.Questions.QuestionGeneration
{
	/// <summary>
	/// Generates a serializable class.
	/// </summary>
	public class JavaClassDefinitionGenerator : IJavaClassDefinitionGenerator
	{
		/// <summary>
		/// The java class.
		/// </summary>
		private readonly JavaFileBuilder _builder;

		/// <summary>
		/// The java class whose definition will be generated.
		/// </summary>
		private readonly JavaClass _javaClass;

		/// <summary>
		/// Constructor.
		/// </summary>
		public JavaClassDefinitionGenerator(JavaFileBuilder builder, JavaClass javaClass)
		{
			_builder = builder;
			_javaClass = javaClass;
		}

		/// <summary>
		/// Generates the given serializable class.
		/// </summary>
		public void GenerateClassDefinition()
		{
			_builder
				.AddLine(GetClassDeclaration())
				.BeginScope("{")
					.AddLines(_javaClass.Properties.Select(GetField))
					.AddBlankLine()
					.AddLines(GetTypeAccessor(_javaClass))
					.AddLines(_javaClass.Properties.Select(GetAccessor))
					.AddBlankLine()
					.AddLine(GetConstructorSignature())
					.BeginScope("{")
						.AddLines(GetConstructorBody())
					.EndScope("}")
				.EndScope("}");
		}

		/// <summary>
		/// Returns the class declaration.
		/// </summary>
		private string GetClassDeclaration()
		{
			var extends = _javaClass.BaseClass != null
				? $" extends {_javaClass.BaseClass.ClassName}"
				: "";

			return $"class {_javaClass.ClassName}{extends}";
		}

		/// <summary>
		/// Returns a field definition.
		/// </summary>
		private string GetField(JavaClassProperty prop)
		{
			return $"private {prop.JavaType} {prop.Name.ToCamelCase()};";
		}

		/// <summary>
		/// Returns an accessor for the string.
		/// </summary>
		private IEnumerable<string> GetTypeAccessor(JavaClass javaClass)
		{
			if (!javaClass.AbstractClass && javaClass.BaseClass != null)
				yield return $"public String getType() {{ return \"{javaClass.ClassName}\"; }}";
		}

		/// <summary>
		/// Returns definitions for each property accessor.
		/// </summary>
		private string GetAccessor(JavaClassProperty prop)
		{
			return $"public {prop.JavaType} get{prop.Name}() {{ return {prop.Name.ToCamelCase()}; }}";
		}

		/// <summary>
		/// Returns the definition for the constructor.
		/// </summary>
		private string GetConstructorSignature()
		{
			return $"public {_javaClass.ClassName}({GetConstructorParams(_javaClass, includeTypes: true)})";
		}

		/// <summary>
		/// Returns a string containing the parameters of the constructor signature.
		/// </summary>
		private string GetConstructorParams(JavaClass javaClass, bool includeTypes)
		{
			var curClassParams = string.Join(
				", ", 
				javaClass.Properties.Select(prop => $"{(includeTypes ? $"{prop.JavaType} " : "")}{prop.Name.ToCamelCase()}"));

			var baseClassParams = javaClass.BaseClass != null
				? GetConstructorParams(javaClass.BaseClass, includeTypes)
				: null;

			if (!string.IsNullOrEmpty(baseClassParams))
				return $"{baseClassParams}, {curClassParams}";
			else
				return curClassParams;
		}

		/// <summary>
		/// Returns the body of the constructor.
		/// </summary>
		private IEnumerable<string> GetConstructorBody()
		{
			if (_javaClass.BaseClass != null)
				yield return $"super({GetConstructorParams(_javaClass.BaseClass, includeTypes: false)});";

			foreach (var prop in _javaClass.Properties)
			{
				yield return $"this.{prop.Name.ToCamelCase()} = {prop.Name.ToCamelCase()};";
			}
		}
	}
}
