package csc.coderunner;

import java.lang.reflect.Constructor;
import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.util.ArrayList;
import java.util.List;

public class MethodDefinition
{
	private String name;
	private List<String> parameterTypes;
	private String returnType;
	private boolean isStatic;
	private boolean isPublic;

	public MethodDefinition(Method method)
	{
		this.name = method.getName();
		
		this.parameterTypes = new ArrayList<String>();
		for (Class<?> parameterType : method.getParameterTypes())
		{
			parameterTypes.add(parameterType.getSimpleName());
		}
		
		this.returnType = method.getReturnType().getSimpleName();
		this.isStatic = Modifier.isStatic(method.getModifiers());
		this.isPublic = Modifier.isPublic(method.getModifiers());
	}

	public MethodDefinition(Constructor<?> constructor)
	{
		this.name = constructor.getName();
		
		this.parameterTypes = new ArrayList<String>();
		for (Class<?> parameterType : constructor.getParameterTypes())
		{
			parameterTypes.add(parameterType.getSimpleName());
		}
		
		this.returnType = null;
		this.isStatic = false;
		this.isPublic = Modifier.isPublic(constructor.getModifiers());
	}

	/**
	 * @return The name of the method.
	 */
	public String getName()
	{
		return name;
	}
	
	/**
	 * @return The parameter types for the method.
	 */
	public List<String> getParameterTypes()
	{
		return parameterTypes;
	}
	
	/**
	 * @return The return type of the method.
	 */
	public String getReturnType()
	{
		return returnType;
	}	
	
	/**
	 * @return Whether or not the method is static.
	 */
	public boolean getIsStatic()
	{
		return isStatic;
	}
	
	/**
	 * @return Whether or not the method is public.
	 */
	public boolean getIsPublic()
	{
		return isPublic;
	}
}
