using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CSC.Common.Infrastructure.Serialization
{
	/// <summary>
	/// Serializes and deserializes json objects for APIs.
	/// </summary>
	public class ApiContractResolver : DefaultContractResolver
	{
		/// <summary>
		/// A list of type maps, keyed by base type. Each entry in a 
		/// type map consists of a string representation of a sub type,
		/// along with the actual subclass type.
		/// </summary>
		private readonly ITypeMapCollection _typeMaps;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="typeMaps">A list of type maps, keyed by base type.
		/// Each entry in a typemap consists of a string representation of 
		/// a sub type, along with the actual subclass type.</param>
		public ApiContractResolver(ITypeMapCollection typeMaps)
		{
			NamingStrategy = new CamelCaseNamingStrategy();
			_typeMaps = typeMaps ?? new TypeMapCollection();
		}

		/// <summary>
		/// Returns a custom json converter for abstract classes.
		/// </summary>
		protected override JsonConverter ResolveContractConverter(Type objectType)
		{
			Type topLevelBaseType = GetInheritanceHierarchy(objectType).First();

			IReadOnlyDictionary<string, Type> typeMap;
			_typeMaps.TryGetValue(topLevelBaseType, out typeMap);

			if (typeMap == null)
				return base.ResolveContractConverter(objectType);

			return new BaseClassJsonConverter(topLevelBaseType, typeMap);
		}

		/// <summary>
		/// Returns a list of json Properties, where 
		/// * Base class properties appear before derived class properties
		/// * Enum values are serialized as strings
		/// </summary>
		protected override IList<JsonProperty> CreateProperties(
			Type type,
			MemberSerialization memberSerialization)
		{
			return base.CreateProperties(type, memberSerialization)
				?.OrderBy(p => GetInheritanceHierarchy(p.DeclaringType).Count)
				?.Select(CamelCaseEnumValues)
				?.ToList();
		}

		/// <summary>
		/// Ensures that enum values are serialized as camel-cased strings.
		/// </summary>
		private static JsonProperty CamelCaseEnumValues(JsonProperty property)
		{
			if (property.PropertyType.GetTypeInfo().IsEnum)
				property.Converter = new StringEnumConverter(camelCaseText: true);

			return property;
		}

		/// <summary>
		/// Returns the inheritance hierarchy, from the top-most base class on down.
		/// </summary>
		private static List<Type> GetInheritanceHierarchy(Type objectType)
		{
			List<Type> hierarchy = new List<Type>() { objectType };
			var baseType = objectType.GetTypeInfo().BaseType;

			while (baseType != null && baseType != typeof(object))
			{
				hierarchy.Add(baseType);
				baseType = baseType.GetTypeInfo().BaseType;
			}

			hierarchy.Reverse();

			return hierarchy;
		}
	}
}
