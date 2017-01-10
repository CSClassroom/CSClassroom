package csc.coderunner;

import java.lang.reflect.Field;
import java.lang.reflect.Modifier;

public class FieldDefinition
{
	private String name;
	private String type;
	private boolean isStatic;
	private boolean isPublic;
	
	public FieldDefinition(Field field)
	{
		this.name = field.getName();
		this.type = field.getType().getName();
		this.isStatic = Modifier.isStatic(field.getModifiers());
		this.isPublic = !Modifier.isPrivate(field.getModifiers()) && !Modifier.isProtected(field.getModifiers());
	}

	/**
	 * @return The name of the field.
	 */
	public String getName()
	{
		return name;
	}
	
	/**
	 * @return The type of the field.
	 */
	public String getType()
	{
		return type;
	}	
	
	/**
	 * @return Whether or not the field is static.
	 */
	public boolean getIsStatic()
	{
		return isStatic;
	}
	
	/**
	 * @return Whether or not the field is private.
	 */
	public boolean getIsPublic()
	{
		return isPublic;
	}
}
