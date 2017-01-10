namespace CSC.CSClassroom.Service.Questions.QuestionGeneration
{
	/// <summary>
	/// Represents a property in a serializable class, backed by 
	/// a field and a getter.
	/// </summary>
	public class JavaClassProperty
	{
		/// <summary>
		/// The property name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The type of the property.
		/// </summary>
		public string JavaType { get; }

		/// <summary>
		/// Whether or not the property is a collection.
		/// </summary>
		public bool IsCollection { get; }

		/// <summary>
		/// The java class of the property, if it is an object type or collection of object types.
		/// </summary>
		public JavaClass JavaClass { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public JavaClassProperty(string name, string javaType, bool isCollection, JavaClass javaClass)
		{
			Name = name;
			JavaType = javaType;
			IsCollection = isCollection;
			JavaClass = javaClass;
		}
	}
}
