using System;
using System.Collections.Generic;

namespace CSC.Common.Infrastructure.Serialization
{
	/// <summary>
	/// A collection of type maps for the JSON serializer.
	/// </summary>
	public class TypeMapCollection : Dictionary<Type, IReadOnlyDictionary<string, Type>>, ITypeMapCollection
	{
	}
}
