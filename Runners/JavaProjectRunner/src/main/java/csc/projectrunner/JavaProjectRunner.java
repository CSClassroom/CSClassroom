package csc.projectrunner;

import java.io.FileOutputStream;
import java.io.IOException;
import java.io.PrintStream;

import org.junit.runner.JUnitCore;

public class JavaProjectRunner
{
	public static void main(String[] args) throws ClassNotFoundException, IOException
	{
		System.setOut(new PrintStream(new NoInterruptionOutputStream(System.out, 0 /*limit*/)));
		System.setErr(new PrintStream(new NoInterruptionOutputStream(System.err, 0 /*limit*/)));
		
		JUnitCore junit = new JUnitCore();
		JsonRunListener listener = new JsonRunListener(new FileOutputStream(args[0]));
		junit.addListener(listener);
		listener.open();
		
		for (String className : args[1].split(";"))
		{
			junit.run(Class.forName(className));			
		}
		
		listener.close();
		
		System.exit(0);
	}
}
