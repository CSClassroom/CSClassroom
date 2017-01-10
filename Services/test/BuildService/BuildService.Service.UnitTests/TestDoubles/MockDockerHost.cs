using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Service.Docker;

namespace CSC.BuildService.Service.UnitTests.TestDoubles
{
	/// <summary>
	/// A mock docker host.
	/// </summary>
	public class MockDockerHost : IDockerHost
	{
		/// <summary>
		/// The docker result to return.
		/// </summary>
		private readonly DockerResult _dockerResult;

		/// <summary>
		/// Whether or not RanImageInNewContainerAsync was called.
		/// </summary>
		public bool RanImage { get; private set; }

		/// <summary>
		/// The contents of the request.
		/// </summary>
		public string RequestContents { get; private set; }

		/// <summary>
		/// The environment variables of the request.
		/// </summary>
		public IDictionary<string, string> EnvironmentVariables { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MockDockerHost(DockerResult expectedResult)
		{
			_dockerResult = expectedResult;
		}

		/// <summary>
		/// Simulates running a container. 
		/// </summary>
		public Task<DockerResult> RunImageInNewContainerAsync(
			string requestContents,
			IDictionary<string, string> environmentVariables)
		{
			if (RanImage)
			{
				throw new InvalidOperationException("Alreeady called.");
			}

			RanImage = true;
			RequestContents = requestContents;
			EnvironmentVariables = environmentVariables;

			return Task.FromResult(_dockerResult);
		}
	}
}
