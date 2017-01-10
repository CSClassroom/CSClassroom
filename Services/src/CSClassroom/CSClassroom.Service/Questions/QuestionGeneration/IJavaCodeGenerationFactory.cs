using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Service.Questions.QuestionGeneration
{
	/// <summary>
	/// Creates classes for java code generation.
	/// </summary>
	public interface IJavaCodeGenerationFactory
	{
		/// <summary>
		/// Creates a java object model builder.
		/// </summary>
		IJavaModelBuilder CreateBuilder(Type modelRootType);

		/// <summary>
		/// Creates a java class definition generator.
		/// </summary>
		IJavaClassDefinitionGenerator CreateJavaClassDefinitionGenerator(
			JavaFileBuilder fileBuilder,
			JavaClass javaClass);

		/// <summary>
		/// Creates a constructor invocation generator.
		/// </summary>
		IJavaConstructorInvocationGenerator CreateConstructorInvocationGenerator(
			JavaFileBuilder fileBuilder,
			IList<JavaClass> javaClasses);
	}
}