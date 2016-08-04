using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSC.Common.Service.Serialization
{
	/// <summary>
	/// Serializes and deserializes json objects for APIs.
	/// </summary>
	public class ApiContractResolver : DefaultContractResolver
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiContractResolver()
		{
			NamingStrategy = new CamelCaseNamingStrategy();
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
