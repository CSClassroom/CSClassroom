using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// Creates classes for java code generation.
	/// </summary>
	public class JavaCodeGenerationFactory : IJavaCodeGenerationFactory
	{
		/// <summary>
		/// Creates a java object model builder.
		/// </summary>
		public IJavaModelBuilder CreateBuilder(Type modelRootType)
		{
			return new JavaModelBuilder(modelRootType);
		}

		/// <summary>
		/// Creates a java class definition generator.
		/// </summary>
		public IJavaClassDefinitionGenerator CreateJavaClassDefinitionGenerator(
			JavaFileBuilder fileBuilder,
			JavaClass javaClass)
		{
			return new JavaClassDefinitionGenerator(fileBuilder, javaClass);
		}

		/// <summary>
		/// Creates a constructor invocation generator.
		/// </summary>
		public IJavaConstructorInvocationGenerator CreateConstructorInvocationGenerator(
			JavaFileBuilder fileBuilder,
			IList<JavaClass> javaClasses)
		{
			return new JavaConstructorInvocationGenerator(fileBuilder, javaClasses);
		}
	}
}
