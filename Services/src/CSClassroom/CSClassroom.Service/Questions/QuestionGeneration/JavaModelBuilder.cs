using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSC.CSClassroom.Service.Questions.QuestionGeneration
{
	/// <summary>
	/// Generates a java object model, corresponding to a C# object model
	/// rooted at a given type.
	/// </summary>
	public class JavaModelBuilder : IJavaModelBuilder
	{
		/// <summary>
		/// Types to exclude from the model.
		/// </summary>
		private readonly ICollection<Type> _typesToExclude
			= new HashSet<Type>();

		/// <summary>
		/// Names of properties to exclude from the model.
		/// </summary>
		private readonly ICollection<Func<PropertyInfo, bool>> _propertiesToExclude
			= new List<Func<PropertyInfo, bool>>();

		/// <summary>
		/// The set of model classes created so far.
		/// </summary>
		private readonly IDictionary<Type, JavaClass> _modelClasses 
			= new Dictionary<Type, JavaClass>();

		/// <summary>
		/// A set of types for which model classes have been seen so far.
		/// </summary>
		private readonly ISet<Type> _modelClassesSeen 
			= new HashSet<Type>();

		/// <summary>
		/// The root type of the model.
		/// </summary>
		private readonly Type _modelRootType;

		/// <summary>
		/// Constructor.
		/// </summary>
		public JavaModelBuilder(Type modelRootType)
		{
			_modelRootType = modelRootType;
		}

		/// <summary>
		/// Exclude a given type from the java model.
		/// </summary>
		public IJavaModelBuilder ExcludeType(Type type)
		{
			_typesToExclude.Add(type);

			return this;
		}
		
		/// <summary>
		/// Exclude a given property from the java model.
		/// </summary>
		public IJavaModelBuilder ExcludeProperties(Func<PropertyInfo, bool> excludeProperty)
		{
			_propertiesToExclude.Add(excludeProperty);

			return this;
		}

		/// <summary>
		/// Generates a java object model, corresponding to a C# object model
		/// rooted at a given type.
		/// </summary>
		public IList<JavaClass> Build()
		{
			EnsureTypeAdded(_modelRootType);

			return _modelClasses
				.Select(kvp => kvp.Value)
				.ToList();
		}

		/// <summary>
		/// Returns the serializable class for the given type (generating
		/// the class if it is not already generated).
		/// </summary>
		private JavaClass GetAndStoreModelClass(Type type)
		{
			if (!_modelClasses.ContainsKey(type))
			{
				if (_modelClassesSeen.Contains(type))
					return null;

				_modelClassesSeen.Add(type);

				_modelClasses[type] = CreateModelClass(type);

				if (type.GetTypeInfo().IsAbstract)
				{
					EnsureSubtypesAdded(type);
				}
			}

			return _modelClasses[type];
		}

		/// <summary>
		/// Creates a model class for the given type.
		/// </summary>
		private JavaClass CreateModelClass(Type type)
		{
			var baseType = type.GetTypeInfo().BaseType;

			return new JavaClass
			(
				type,
				baseType != typeof(object)
					? GetAndStoreModelClass(baseType)
					: null,
				type.GetTypeInfo().DeclaredProperties
					.Where(IncludeProperty)
					.Select(CreateJavaProperty)
					.ToList()
			);
		}

		/// <summary>
		/// Returns whether or not to include the property.
		/// </summary>
		private bool IncludeProperty(PropertyInfo prop)
		{
			return prop.GetGetMethod() != null 
				&& prop.GetSetMethod() != null
				&& _propertiesToExclude.All(excludeProp => !excludeProp(prop))
				&& GetJavaTypeName(prop.PropertyType) != null;
		}

		/// <summary>
		/// Creates a serializable property for the given c# property.
		/// </summary>
		private JavaClassProperty CreateJavaProperty(PropertyInfo prop)
		{
			return new JavaClassProperty(
				prop.Name,
				GetJavaTypeName(prop.PropertyType),
				IsSupportedCollectionType(prop.PropertyType),
				GetJavaClassForProperty(prop.PropertyType));
		}

		/// <summary>
		/// Returns the java type name, if the type is supported.
		/// </summary>
		private string GetJavaTypeName(Type type)
		{
			if (type == typeof(string))
				return "String";
			else if (type == typeof(int))
				return "int";
			else if (type == typeof(bool))
				return "boolean";
			else if (type == typeof(double))
				return "double";
			else if (IsSupportedCollectionType(type))
				return $"List<{type.GetTypeInfo().GetGenericArguments()[0].Name}>";
			else if (IsSupportedClassType(type))
				return type.Name;
			else
				return null; // not supported
		}

		/// <summary>
		/// Returns the java class for the given property, for object properties
		/// and collections of object properties.
		/// </summary>
		private JavaClass GetJavaClassForProperty(Type type)
		{
			if (IsSupportedClassType(type))
				return GetAndStoreModelClass(type);
			if (IsSupportedCollectionType(type))
				return GetAndStoreModelClass(type.GetTypeInfo().GetGenericArguments()[0]);
			else
				return null;
		}

		/// <summary>
		/// Returns whether or not the given type is in the same assembly as the root type.
		/// </summary>
		private bool IsSupportedClassType(Type type)
		{
			return type.GetTypeInfo().IsClass
				&& _modelRootType.GetTypeInfo().Assembly.Equals(type.GetTypeInfo().Assembly);
		}

		/// <summary>
		/// Returns whether the given type is a supported collection type.
		/// </summary>
		private bool IsSupportedCollectionType(Type type)
		{
			// Perf optimization: string is an enumerable of chars, that happens
			// to implement many interfaces. Just bail here if it is a string.
			if (type == typeof(string))
				return false;

			return type.GetInterfaces()
			   .Any(t => t.GetTypeInfo().IsGenericType
				   && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)
				   && IsSupportedClassType(type.GetTypeInfo().GetGenericArguments()[0]));
		}

		/// <summary>
		/// Ensures the given type is added to our model.
		/// </summary>
		private void EnsureTypeAdded(Type type)
		{
			GetAndStoreModelClass(type);
		}

		/// <summary>
		/// Ensures all subtypes of a given type are added to our model.
		/// </summary>
		private void EnsureSubtypesAdded(Type type)
		{
			var subTypes = type.GetTypeInfo()
				.Assembly
				.GetTypes()
				.Where(t => t.GetTypeInfo().BaseType == type)
				.Where(t => !_typesToExclude.Contains(t))
				.ToList();

			foreach (var subType in subTypes)
			{
				EnsureTypeAdded(subType);
			}
		}
	}
}
