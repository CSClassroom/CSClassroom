using System;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The progress of a build job.
	/// </summary>
	public class BuildProgress
	{
		/// <summary>
		/// The duration of the build, in seconds.
		/// </summary>
		public int? Duration { get; }

		/// <summary>
		/// Whether or not the build has been enqueued.
		/// </summary>
		public bool Enqueued { get; }

		/// <summary>
		/// Whether or not the build has completed.
		/// </summary>
		public bool Completed { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		private BuildProgress(int? duration, bool enqueued, bool completed)
		{
			Duration = duration;
			Enqueued = enqueued;
			Completed = completed;
		}

		/// <summary>
		/// Indicates the build has been queued.
		/// </summary>
		public static BuildProgress EnqueuedBuild()
		{
			return new BuildProgress(duration: null, enqueued: true, completed: false);
		}

		/// <summary>
		/// Indicates the build is in progress.
		/// </summary>
		public static BuildProgress InProgressBuild(TimeSpan duration)
		{
			return new BuildProgress((int)duration.TotalSeconds, enqueued: true, completed: false);
		}

		/// <summary>
		/// Indicates the build is complete.
		/// </summary>
		public static BuildProgress CompletedBuild()
		{
			return new BuildProgress(duration: null, enqueued: true, completed: true);
		}

		/// <summary>
		/// Indicates the build status is unknown.
		/// </summary>
		public static BuildProgress UnknownBuild()
		{
			return new BuildProgress(duration: null, enqueued: false, completed: false);
		}
	}
}
