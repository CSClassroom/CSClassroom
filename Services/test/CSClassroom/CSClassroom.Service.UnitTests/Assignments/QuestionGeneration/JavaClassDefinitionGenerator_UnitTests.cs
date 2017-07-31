using System.Linq;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionGeneration
{
	/// <summary>
	/// Unit tests for the JavaClassDefinitionGenerator class.
	/// </summary>
	public class JavaClassDefinitionGenerator_UnitTests
	{
		/// <summary>
		/// Ensures that GenerateClassDefinition generates the correct definition.
		/// </summary>
		[Fact]
		public void GenerateClassDefinition_GeneratesCorrectDefinition()
		{
			var javaClass = ExpectedJavaModel.GetFullJavaModel()
				.Single(c => c.ClassName == "RootObject");

			var fileBuilder = new JavaFileBuilder();
			var classDefGenerator = new JavaClassDefinitionGenerator(fileBuilder, javaClass);
			
			classDefGenerator.GenerateClassDefinition();

			var result = fileBuilder.GetFileContents();
			var expectedResult = c_correctDefinition.Replace("\r\n", "\n");

			Assert.Equal(expectedResult, result);
		}

		private const string c_correctDefinition =
@"class RootObject
{
	private String stringProp;
	private int intProp;
	private boolean boolProp;
	private BaseChildObject singleBaseChildObject;
	private List<BaseChildObject> baseChildObjectList;
	private DerivedChildObject1 singleDerivedChildObject;
	private List<DerivedChildObject2> derivedChildObjectList;

	public String getStringProp() { return stringProp; }
	public int getIntProp() { return intProp; }
	public boolean getBoolProp() { return boolProp; }
	public BaseChildObject getSingleBaseChildObject() { return singleBaseChildObject; }
	public List<BaseChildObject> getBaseChildObjectList() { return baseChildObjectList; }
	public DerivedChildObject1 getSingleDerivedChildObject() { return singleDerivedChildObject; }
	public List<DerivedChildObject2> getDerivedChildObjectList() { return derivedChildObjectList; }

	public RootObject(String stringProp, int intProp, boolean boolProp, BaseChildObject singleBaseChildObject, List<BaseChildObject> baseChildObjectList, DerivedChildObject1 singleDerivedChildObject, List<DerivedChildObject2> derivedChildObjectList)
	{
		this.stringProp = stringProp;
		this.intProp = intProp;
		this.boolProp = boolProp;
		this.singleBaseChildObject = singleBaseChildObject;
		this.baseChildObjectList = baseChildObjectList;
		this.singleDerivedChildObject = singleDerivedChildObject;
		this.derivedChildObjectList = derivedChildObjectList;
	}
}
";
	}
}
