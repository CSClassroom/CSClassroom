package csc.projectrunner;

import java.io.FilterOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;

public class NoInterruptionOutputStream extends FilterOutputStream 
{
	private int numBytesOutput;
	private int limit;
	private boolean reachedLimit;
	
	public NoInterruptionOutputStream(OutputStream outputStream, int limit) 
	{
		super(outputStream);
		this.limit = limit;
	}

	public void write(int b) throws IOException
	{
		if (Thread.interrupted())
		{
			throw new RuntimeException(new InterruptedException());
		}
		
		if (!reachedLimit(1 /*numBytesToPrint*/))
		{
			out.write(b);
			numBytesOutput += 1;
		}
		else
		{
			handleLimitReached();
		}
	}

	public void write(byte[] b) throws IOException 
	{
		if (Thread.interrupted())
		{
			throw new RuntimeException(new InterruptedException());
		}
		
		if (!reachedLimit(b.length))
		{
			out.write(b, 0, b.length);
			numBytesOutput += b.length;
		}
		else
		{
			handleLimitReached();
		}
	}

	public void write(byte[] b, int off, int len) throws IOException 
	{
		if (Thread.interrupted())
		{
			throw new RuntimeException(new InterruptedException());
		}
		
		if (!reachedLimit(len))
		{
			out.write(b, off, len);
			numBytesOutput += len;
		}
		else
		{
			handleLimitReached();
		}
	}
	
	public String getContents()
	{
		return out.toString();
	}
	
	private boolean reachedLimit(int numBytesToPrint)
	{
		return limit > 0 && (reachedLimit || (numBytesOutput + numBytesToPrint > limit));
	}
	
	public void handleLimitReached()
	{
		if (!reachedLimit)
		{
			new PrintStream(out).println("\n\n*** Test output truncated. ***");
			reachedLimit = true;
		}
	}
}
