package csc.projectrunner;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;
import java.util.ArrayList;
import java.util.Arrays;

import org.junit.runner.Description;
import org.junit.runner.notification.Failure;
import org.junit.runner.notification.RunListener;

import com.fasterxml.jackson.core.JsonFactory;
import com.fasterxml.jackson.core.JsonGenerator;

public class JsonRunListener extends RunListener
{
	private JsonGenerator jsonGen;
	private PrintStream oldSystemOut;
	private PrintStream oldSystemErr;
	private NoInterruptionOutputStream utos;
	private final int c_maxOutputLength = 200000;
	
	public JsonRunListener(OutputStream stream) throws IOException
	{
		JsonFactory factory = new JsonFactory();
		jsonGen = factory.createGenerator(stream);
	}
	
	public void open() throws IOException
	{	
		jsonGen.writeStartArray();
	}

	@Override
	public void testStarted(Description description) throws IOException
	{
		try
		{
			System.out.println("Running test " + description.getClassName() + "." + description.getMethodName());
			
			jsonGen.writeStartObject();
			jsonGen.writeStringField("className", description.getClassName());		
			jsonGen.writeStringField("testName", description.getMethodName());
			
			utos = new NoInterruptionOutputStream(new ByteArrayOutputStream(), c_maxOutputLength);
			
			oldSystemOut = System.out;
			oldSystemErr = System.err;
			
			System.setOut(new PrintStream(utos));
			System.setErr(new PrintStream(utos));
		}
		catch (Exception ex)
		{
			ex.printStackTrace(oldSystemOut);
			System.exit(1);
		}
	}

	@Override
	public void testFinished(Description description) throws IOException
	{
		try
		{
			jsonGen.writeEndObject();
			
			utos.close();
			utos = null;
			
			System.setOut(oldSystemOut);
			System.setErr(oldSystemErr);
		}
		catch (Exception ex)
		{
			ex.printStackTrace(oldSystemOut);
			System.exit(1);
		}
	}

	@Override
	public void testFailure(Failure failure) throws IOException
	{
		try
		{
			if (failure.getException().getClass().equals(OutOfMemoryError.class))
			{
				failure.getException().printStackTrace(oldSystemOut);
				System.exit(0);
			}
			
			jsonGen.writeFieldName("failure");
			
			jsonGen.writeStartObject();
			
			jsonGen.writeStringField("message", getFailureMessage(failure));
			jsonGen.writeStringField("trace", getFilteredStackTrace(failure));
			
			utos.flush();
			
			jsonGen.writeStringField("output", utos.getContents());
			
			jsonGen.writeEndObject();
		}
		catch (Exception ex)
		{
			ex.printStackTrace(oldSystemOut);
			System.exit(1);
		}
	}

	@Override
	public void testAssumptionFailure(Failure failure)
	{
	}

	@Override
	public void testIgnored(Description description) throws IOException
	{
	}
	
	public void close() throws IOException
	{
		jsonGen.writeEndArray();
		
		jsonGen.close();
	}
	
	private String getFailureMessage(Failure failure)
	{
		String message = failure.getException().getClass().getName() + ": " + failure.getMessage();
		if (message != null && message.length() > c_maxOutputLength)
		{
			message = message.substring(0, c_maxOutputLength)
				+ "\n\n*** Failure message truncated ***";
		}
		
		return message;
	}
	
	private String getFilteredStackTrace(Failure failure)
	{
		StackTraceElement[] frames = failure.getException().getStackTrace();
		
		int startingFrameIndex = frames.length - 1;
		for (int frameIndex = frames.length - 1; frameIndex >= 0; frameIndex--)
		{
			if (includeFrame(frames[frameIndex]))
			{
				startingFrameIndex = frameIndex;
				break;
			}
		}
		
		return formatStackTrace(Arrays.copyOfRange(frames, 0, startingFrameIndex + 1));
	}
	
	private boolean includeFrame(StackTraceElement frame)
	{
		String[] packagesToIgnore = 
		{
			"csc.projectrunner",
			"org.junit",
			"java.lang.reflect",
			"sun.reflect"
		};
		
		for (String packageToIgnore : packagesToIgnore)
		{
			if (frame.getClassName().startsWith(packageToIgnore))
				return false;
		}
		
		return true;
	}
	
	public String formatStackTrace(StackTraceElement[] frames)
	{
		StringBuilder sb = new StringBuilder();
		for(int frameIndex = 0; frameIndex < frames.length; frameIndex++)
		{
			StackTraceElement  frame = frames[frameIndex];
			
			String formattedFrame = String.format(
				"%s.%s (%s:%d)",
				frame.getClassName(),
				frame.getMethodName(),
				frame.getFileName(),
				frame.getLineNumber());
			
			sb.append(formattedFrame);
			
			if (frameIndex < frames.length - 1)
			{
				sb.append("\n");
			}
		}
		
		return sb.toString();
	}
}
