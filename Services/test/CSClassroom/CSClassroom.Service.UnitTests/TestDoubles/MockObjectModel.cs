using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.UnitTests.Utilities;

namespace CSC.CSClassroom.Service.UnitTests.TestDoubles
{
	/// <summary>
	/// The root of an object model.
	/// </summary>
	public class RootObject
	{
		public string StringProp { get; set; }
		public int IntProp { get; set; }
		public bool BoolProp { get; set; }
		public BaseChildObject SingleBaseChildObject { get; set; }
		public List<BaseChildObject> BaseChildObjectList { get; set; }
		public DerivedChildObject1 SingleDerivedChildObject { get; set; }
		public List<DerivedChildObject2> DerivedChildObjectList { get; set; }
	}

	/// <summary>
	/// A base class for child objects in the object model.
	/// </summary>
	public abstract class BaseChildObject
	{
		public string StringProp { get; set; }
	}

	/// <summary>
	/// A derived child object class.
	/// </summary>
	public class DerivedChildObject1 : BaseChildObject
	{
		public int IntProp { get; set; }
	}

	/// <summary>
	/// A derived child object class.
	/// </summary>
	public class DerivedChildObject2 : BaseChildObject
	{
		public bool BoolProp { get; set; }
	}
	
	/// <summary>
	/// Utility methods for java generation tests.
	/// </summary>
	public static class ExpectedJavaModel
	{
		/// <summary>
		/// Returns the full java model for the object graph
		/// rooted at RootObject.
		/// </summary>
		public static IList<JavaClass> GetFullJavaModel()
		{
			var baseClass = new JavaClass
			(
				typeof(BaseChildObject),
				null /*baseClass*/,
				Collections.CreateList
				(
					new JavaClassProperty("StringProp", "String", isCollection: false, javaClass: null)
				)	
			);

			var derivedClass1 = new JavaClass
			(
				typeof(DerivedChildObject1),
				baseClass,
				Collections.CreateList
				(
					new JavaClassProperty("IntProp", "int", isCollection: false, javaClass: null)
				)	
			);

			var derivedClass2 = new JavaClass
			(
				typeof(DerivedChildObject2),
				baseClass,
				Collections.CreateList
				(
					new JavaClassProperty("BoolProp", "boolean", isCollection: false, javaClass: null)
				)
			);

			var rootClass1 = new JavaClass
			(
				typeof(RootObject),
				null /*baseClass*/,
				Collections.CreateList
				(
					new JavaClassProperty("StringProp", "String", isCollection: false, javaClass: null),
					new JavaClassProperty("IntProp", "int", isCollection: false, javaClass: null),
					new JavaClassProperty("BoolProp", "boolean", isCollection: false, javaClass: null),
					new JavaClassProperty("SingleBaseChildObject", "BaseChildObject", isCollection: false, javaClass: baseClass),
					new JavaClassProperty("BaseChildObjectList", "List<BaseChildObject>", isCollection: true, javaClass: baseClass),
					new JavaClassProperty("SingleDerivedChildObject", "DerivedChildObject1", isCollection: false, javaClass: derivedClass1),
					new JavaClassProperty("DerivedChildObjectList", "List<DerivedChildObject2>", isCollection: true, javaClass: derivedClass2)
				)
			);

			return Collections.CreateList
			(
				rootClass1,
				baseClass,
				derivedClass1,
				derivedClass2
			);
		}
	}
}
