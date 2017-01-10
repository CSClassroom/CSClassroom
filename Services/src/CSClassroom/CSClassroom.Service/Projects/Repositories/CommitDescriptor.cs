using System;

namespace CSC.CSClassroom.Service.Projects.Repositories
{
	/// <summary>
	/// A description of a commit.
	/// </summary>
	public class CommitDescriptor
	{
		/// <summary>
		/// The SHA of the commit.
		/// </summary>
		public string Sha { get; }

		/// <summary>
		/// The Project ID for the commit.
		/// </summary>
		public int ProjectId { get; }

		/// <summary>
		/// The user for the commit.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CommitDescriptor(String sha, int projectId, int userId)
		{
			Sha = sha;
			ProjectId = projectId;
			UserId = userId;
		}

		/// <summary>
		/// Returns the hash code.
		/// </summary>
		public override int GetHashCode()
		{
			return Sha.GetHashCode() ^ ProjectId.GetHashCode() ^ UserId.GetHashCode();
		}

		/// <summary>
		/// Returns whether or not this object is equal to another object.
		/// </summary>
		public override bool Equals(object obj)
		{
			CommitDescriptor cd = obj as CommitDescriptor;
			if (cd == null)
				return false;

			return cd.Sha == Sha && cd.ProjectId == ProjectId && cd.UserId == UserId;
		}
	}
}