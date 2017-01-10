using System;
using Autofac;
using Hangfire;

namespace CSC.BuildService.Endpoint.Queue
{
	/// <summary>
	/// A job activator using an AutoFac container.
	/// </summary>
	public class ContainerJobActivator : JobActivator
	{
		/// <summary>
		/// An AutoFac container.
		/// </summary>
		private IContainer _container;

		/// <summary>
		/// Constructor/
		/// </summary>
		public ContainerJobActivator(IContainer container)
		{
			_container = container;
		}

		/// <summary>
		/// Activates a job.
		/// </summary>
		public override object ActivateJob(Type type)
		{
			return _container.Resolve(type);
		}
	}
}
