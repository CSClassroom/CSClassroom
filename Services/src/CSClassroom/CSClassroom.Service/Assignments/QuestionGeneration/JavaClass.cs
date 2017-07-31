using System;
using System.Collections.Generic;
using System.Reflection;

namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// Represents a data-only java class, with accessors.
	/// </summary>
	public class JavaClass
	{
		/// <summary>
		/// The C# type corresponding to this java class.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// The base class, if any.
		/// </summary>
		public JavaClass BaseClass { get; }

		/// <summary>
		/// The properties on this class to serialize.
		/// </summary>
		public IList<JavaClassProperty> Properties { get; }

		/// <summary>
		/// The name of the class.
		/// </summary>
		public string ClassName => Type.Name;

		/// <summary>
		/// Whether or not the class is abstract.
		/// </summary>
		public bool AbstractClass => Type.GetTypeInfo().IsAbstract;

		/// <summary>
		/// Constructor.
		/// </summary>
		public JavaClass(
			Type type,
			JavaClass baseClass,
			IList<JavaClassProperty> properties)
		{
			Type = type;
			BaseClass = baseClass;
			Properties = properties;
		}
	}
}
