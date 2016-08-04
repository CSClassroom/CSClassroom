package csc.coderunner;

import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;

public class ClassDefinition
{
	/** The name of the class. */
	private String name;
	
	/** The fields on the class. */
	private List<FieldDefinition> fields;
	
	/** The methods on the class. */
	private List<MethodDefinition> methods;
	
	public ClassDefinition(Class<?> javaClass)
	{
		this.name = javaClass.getName();
		
		this.fields = new ArrayList<FieldDefinition>();
		for (Field field : javaClass.getDeclaredFields())
		{
			fields.add(new FieldDefinition(field));
		}
		
		this.methods = new ArrayList<MethodDefinition>();
		for (Constructor<?> constructor : javaClass.getDeclaredConstructors())
		{
			methods.add(new MethodDefinition(constructor));
		}
		for (Method method : javaClass.getDeclaredMethods())
		{
			methods.add(new MethodDefinition(method));
		}
	}
	
	/**
	 * @return The name of the class.
	 */
	public String getName()
	{
		return name;
	}
	
	/**
	 * @return The fields on the class.
	 */
	public List<FieldDefinition> getFields()
	{
		return fields;
	}
	
	/**
	 * @return The methods on the class.
	 */
	public List<MethodDefinition> getMethods()
	{
		return methods;
	}
}
