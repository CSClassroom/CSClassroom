package csc.coderunner;

import com.fasterxml.jackson.core.JsonParseException;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import java.io.File;
import java.io.IOException;

public class JavaCodeRunner
{
	/**
	 * Validates the arguments passed to JavaCodeRunner.
	 * @param args The arguments to validate.
	 * @return Whether or not the arguments are valid.
	 */
	public static boolean validateArguments(String[] args)
	{
		if (args.length != 3)
			return false;

		if (!args[0].equals("classJob") && !args[0].equals("methodJob"))
			return false;

		return true;
	}

	/**
	 * Reads a code job from an input file, and writes the result to an output file.
	 * @param args Command line arguments.
	 * @throws JsonParseException Thrown if the input file could not
	 * be parsed.
	 * @throws JsonMappingException Thrown if the json could not be 
	 * mapped to known classes.
	 * @throws IOException Thrown if the input file could not be read, 
	 * or if the output file could not be written.
	 */
	public static void main(String[] args) throws JsonParseException, JsonMappingException, IOException
	{
		if (!validateArguments(args))
		{
			System.out.println("Usage: JavaCodeRunner [ classJob | methodJob ] input-file output-file");
			return;
		}
		
		String jobType = args[0];
		String inputFile = args[1];
		String outputFile = args[2];
		
		ObjectMapper mapper = new ObjectMapper();
		
		CodeJob<?> job = jobType.equals("classJob")
			? mapper.readValue(new File(inputFile), ClassJob.class)
			: mapper.readValue(new File(inputFile), MethodJob.class);
			
		CodeJobResult result = job.runJob();
		mapper.writerWithDefaultPrettyPrinter().writeValue(new File(outputFile), result);
	}
}