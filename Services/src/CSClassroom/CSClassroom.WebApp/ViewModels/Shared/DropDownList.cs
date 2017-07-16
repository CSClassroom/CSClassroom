using System;
using System.Collections.Generic;
using System.Linq;

namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// A drop-down list item.
	/// </summary>
	public struct DropDownListItem : IEquatable<DropDownListItem>
	{
		/// <summary>
		/// The name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The value.
		/// </summary>
		public int Value { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public DropDownListItem(string name, int value)
		{
			Name = name;
			Value = value;
		}

		/// <summary>
		/// Compares two items for equality.
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is DropDownListItem)
				return Equals((DropDownListItem)obj);

			return false;
		}

		/// <summary>
		/// Compares two items for equality.
		/// </summary>
		public bool Equals(DropDownListItem other)
		{
			return Name == other.Name && Value == other.Value;
		}

		/// <summary>
		/// Returns the hash code of this drop down item.
		/// </summary>
		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ Value.GetHashCode();
		}
	}

	/// <summary>
	/// Options to configure a drop-down list in a dynamic table.
	/// </summary>
	public class DropDownList
	{
		/// <summary>
		/// The property name for the item being selected.
		/// </summary>
		public string PropertyName { get; }

		/// <summary>
		/// The choices for the list.
		/// </summary>
		public IList<object> Choices { get; }

		/// <summary>
		/// Creates a drop-down list item for the given choice.
		/// </summary>
		public Func<object, DropDownListItem> ItemAccessor { get; }

		/// <summary>
		/// The name of the group column (if any).
		/// </summary>
		public string GroupColumnDisplayName { get; }

		/// <summary>
		/// The group filter (if any).
		/// </summary>
		public Func<object, DropDownListItem> GroupFilter { get; }

		/// <summary>
		/// Prevent the modification of the drop-down list value after creation.
		/// </summary>
		public bool PreventModificationAfterCreation { get; }

		/// <summary>
		/// Returns a valid javascript name for a column with the given display name.
		/// </summary>
		public string GroupColumnName => new string
		(
			GroupColumnDisplayName.Where(char.IsLetterOrDigit)
				.ToArray()
		);

		/// <summary>
		/// Constructor.
		/// </summary>
		public DropDownList(
			string propertyName,
			IList<object> choices, 
			Func<object, DropDownListItem> itemAccessor,
			bool preventModificationAfterCreation = false)
		{
			PropertyName = propertyName;
			Choices = choices;
			ItemAccessor = itemAccessor;
			PreventModificationAfterCreation = preventModificationAfterCreation;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DropDownList(
			string propertyName,
			IList<object> choices,
			Func<object, DropDownListItem> itemAccessor,
			string groupColumnName,
			Func<object, DropDownListItem> groupFilter,
			bool preventModificationAfterCreation) 
				: this(propertyName, choices, itemAccessor, preventModificationAfterCreation)
		{
			GroupColumnDisplayName = groupColumnName;
			GroupFilter = groupFilter;
		}
	}
}
