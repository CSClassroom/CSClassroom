using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionGeneration
{
	/// <summary>
	/// Unit tests for the JavaModelBuilder class.
	/// </summary>
	public class JavaModelBuilder_UnitTests
	{
		/// <summary>
		/// Ensures that JavaModelBuilder builds the correct model.
		/// </summary>
		[Fact]
		public void JavaModelBuilder_BuildsCorrectModel()
		{
			var javaModelBuilder = new JavaModelBuilder(typeof(RootObject));

			var expectedJavaModel = ExpectedJavaModel.GetFullJavaModel();
			var actualJavaModel = javaModelBuilder.Build();

			VerifyJavaModel(expectedJavaModel, actualJavaModel);
		}

		/// <summary>
		/// Verifies that the resulting java model matches our expectations.
		/// </summary>
		private void VerifyJavaModel(
			IList<JavaClass> expectedModel, 
			IList<JavaClass> actualModel)
		{
			Assert.Equal(expectedModel.Count, actualModel.Count);

			foreach (var actualJavaClass in actualModel)
			{
				var expectedJavaClass = expectedModel
					.Single(c => c.Type == actualJavaClass.Type);

				VerifyJavaClass(expectedJavaClass, actualJavaClass);
			}
		}

		/// <summary>
		/// Verifies that the resulting java class matches our expectations.
		/// </summary>
		private void VerifyJavaClass(
			JavaClass expectedJavaClass,
			JavaClass actualJavaClass)
		{
			Assert.Equal(expectedJavaClass.Type, actualJavaClass.Type);
			Assert.Equal(expectedJavaClass.ClassName, actualJavaClass.ClassName);
			Assert.Equal(expectedJavaClass.AbstractClass, actualJavaClass.AbstractClass);
			Assert.Equal(expectedJavaClass.BaseClass?.ClassName, actualJavaClass.BaseClass?.ClassName);

			var expectedJavaProps = expectedJavaClass.Properties;
			var actualJavaProps = actualJavaClass.Properties;

			Assert.Equal(expectedJavaProps.Count, actualJavaProps.Count);

			foreach (var actualJavaProp in actualJavaClass.Properties)
			{
				var expectedJavaProp = expectedJavaProps
					.Single(c => c.Name == actualJavaProp.Name);

				VerifyJavaProperty(expectedJavaProp, actualJavaProp);
			}
		}

		/// <summary>
		/// Verifies that the resulting java property matches our expectations.
		/// </summary>
		private void VerifyJavaProperty(
			JavaClassProperty expectedJavaProp,
			JavaClassProperty actualJavaProp)
		{
			Assert.Equal(expectedJavaProp.Name, actualJavaProp.Name);
			Assert.Equal(expectedJavaProp.JavaType, actualJavaProp.JavaType);
			Assert.Equal(expectedJavaProp.IsCollection, actualJavaProp.IsCollection);
			Assert.Equal(expectedJavaProp.JavaClass?.ClassName, actualJavaProp.JavaClass?.ClassName);
		}
	}
}
