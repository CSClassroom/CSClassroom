using System.Collections.Generic;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionGeneration
{
	/// <summary>
	/// Unit tests for the JavaConstructorInvocationGenerator class.
	/// </summary>
	public class JavaConstructorInvocationGenerator_UnitTests
	{
		/// <summary>
		/// Ensures that GenerateConstructorInvocation generates the correct
		/// invocation.
		/// </summary>
		[Fact]
		public void GenerateConstructorInvocation_GeneratesCorrectInvocation()
		{
			var fileBuilder = new JavaFileBuilder();
			var javaModel = ExpectedJavaModel.GetFullJavaModel();
			var constructorInvocationGen = new JavaConstructorInvocationGenerator
			(
				fileBuilder,
				javaModel
			);

			constructorInvocationGen.GenerateConstructorInvocation
			(
				GetRootObject(),
				"return ",
				";"
			);

			var result = fileBuilder.GetFileContents();
			var expectedResult = c_correctInvocation.Replace("\r\n", "\n");

			Assert.Equal(expectedResult, result);
		}

		/// <summary>
		/// Returns a populated root object.
		/// </summary>
		private RootObject GetRootObject()
		{
			return new RootObject()
			{
				StringProp = "StringValue1",
				IntProp = 5,
				BoolProp = true,
				SingleBaseChildObject = new DerivedChildObject1()
				{
					StringProp = "StringValue2",
					IntProp = 8
				},
				SingleDerivedChildObject = new DerivedChildObject1()
				{
					StringProp = "StringValue3",
					IntProp = 9
				},
				BaseChildObjectList = new List<BaseChildObject>()
				{
					new DerivedChildObject1()
					{
						StringProp = "StringValue4",
						IntProp = 100
					},
					new DerivedChildObject2()
					{
						StringProp = "Line one of multiline string\n"
							+ "Line two of multiline string\n"
							+ "Line three of multiline string",
						BoolProp = false
					}
				},
				DerivedChildObjectList = new List<DerivedChildObject2>()
				{
					new DerivedChildObject2()
					{
						StringProp = "StringValue5",
						BoolProp = true
					}
				}
			};
		}

		private const string c_correctInvocation =
@"return new RootObject
(
	/* StringProp: */ ""StringValue1"",
	/* IntProp: */ 5,
	/* BoolProp: */ true,
	/* SingleBaseChildObject: */ new DerivedChildObject1
	(
		/* StringProp: */ ""StringValue2"",
		/* IntProp: */ 8
	),
	/* BaseChildObjectList: */ Arrays.asList(new BaseChildObject[]
	{
		new DerivedChildObject1
		(
			/* StringProp: */ ""StringValue4"",
			/* IntProp: */ 100
		),
		new DerivedChildObject2
		(
			/* StringProp: */ ""Line one of multiline string\n""
				+ ""Line two of multiline string\n""
				+ ""Line three of multiline string"",
			/* BoolProp: */ false
		)
	}),
	/* SingleDerivedChildObject: */ new DerivedChildObject1
	(
		/* StringProp: */ ""StringValue3"",
		/* IntProp: */ 9
	),
	/* DerivedChildObjectList: */ Arrays.asList(new DerivedChildObject2[]
	{
		new DerivedChildObject2
		(
			/* StringProp: */ ""StringValue5"",
			/* BoolProp: */ true
		)
	})
);
";
	}
}
