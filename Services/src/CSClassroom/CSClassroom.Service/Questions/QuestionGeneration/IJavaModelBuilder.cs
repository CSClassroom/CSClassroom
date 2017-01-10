using System;
using System.Collections.Generic;
using System.Reflection;

namespace CSC.CSClassroom.Service.Questions.QuestionGeneration
{
	/// <summary>
	/// Generates a java object model, corresponding to a C# object model
	/// rooted at a given type.
	/// </summary>
	public interface IJavaModelBuilder
	{
		/// <summary>
		/// Exclude a given type from the java model.
		/// </summary>
		IJavaModelBuilder ExcludeType(Type type);

		/// <summary>
		/// Exclude proerties from the java model.
		/// </summary>
		IJavaModelBuilder ExcludeProperties(Func<PropertyInfo, bool> excludeProperty);

		/// <summary>
		/// Generates a java object model, corresponding to a C# object model.
		/// </summary>
		IList<JavaClass> Build();
	}
}