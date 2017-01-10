using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.Utilities;
using CSC.CSClassroom.WebApp.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;

namespace CSC.CSClassroom.WebApp.ViewComponents.Shared
{
	/// <summary>
	/// Renders a dynamic table.
	/// </summary>
	public class DynamicTableViewComponent : ViewComponent
	{
		/// <summary>
		/// The HTTP context accessor.
		/// </summary>
		private ITimeZoneProvider _timeZoneProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DynamicTableViewComponent(ITimeZoneProvider timeZoneProvider)
		{
			_timeZoneProvider = timeZoneProvider;
		}

		/// <summary>
		/// Invokes the view component.
		/// </summary>
		/// <param name="tableElementId">The ID of the table DOM element.</param>
		/// <param name="modelExplorer">The model explorer for the collection containing the table contents.</param>
		/// <param name="properties">The name of the properties of the model object to show in the table.</param>
		/// <param name="hiddenValues">Hidden values that should be included with the properties.</param>
		/// <param name="orderByProp">The property to order the rows by.</param>
		/// <param name="textAreas">Whether to render the properties in multi-line text areas.</param>
		/// <param name="startMinRows">The minimum number of rows in the table to start with.</param>
		/// <param name="defaultValues">The default values for each column, if any.</param>
		/// <param name="dropDownLists">The drop down lists to use, if any.</param>
		/// <param name="subPanelConfig">The configuration object for the subpanel (if any).</param>
		public IViewComponentResult Invoke(
			string tableElementId,
			ModelExplorer modelExplorer,
			string[] properties,
			IDictionary<string, string> hiddenValues,
			string orderByProp,
			int startMinRows,
			bool textAreas,
			IDictionary<string, object> defaultValues,
			IList<DropDownList> dropDownLists,
			SubPanelConfig subPanelConfig)
		{
			var props = properties.Where(prop => prop != null).ToArray();
			var collection = GetOrderedCollection(modelExplorer, orderByProp);
			var elementMetadata = modelExplorer.Metadata.ElementMetadata;
			string collectionName = modelExplorer.Metadata.PropertyName;

			JArray columns = GetColumnInfo
			(
				props,
				hiddenValues,
				textAreas,
				defaultValues,
				dropDownLists,
				elementMetadata
			);

			var initDataProperties = subPanelConfig == null
				? props
				: props.Union(new[] { subPanelConfig.ContentsPropertyName });

			JArray initData = GetInitialData
			(
				hiddenValues, 
				collection,
				dropDownLists,
				elementMetadata, 
				initDataProperties
			);

			return View
			(
				new DynamicTableConfig
				(
					tableElementId, 
					collectionName, 
					columns, 
					initData, 
					startMinRows, 
					textAreas, 
					dropDownLists,
					subPanelConfig
				)
			);
		}

		/// <summary>
		/// Returns the column information for the table.
		/// </summary>
		private JArray GetColumnInfo(
			string[] properties, 
			IDictionary<string, string> hiddenValues,
			bool textAreas,
			IDictionary<string, object> defaultValues,
			IList<DropDownList> dropDownLists, 
			ModelMetadata elementMetadata)
		{
			return new JArray
			(
				new List<JObject>() { GetIdColumnDefinition() }
					.Union
					(
						properties
							.Select(propName => elementMetadata.Properties[propName])
							.Select
							(
								propMetadata => new
								{
									Metadata = propMetadata,
									DropDownList = GetDropDownList(propMetadata, dropDownLists)
								}
							).SelectMany
							(
								p => p.DropDownList != null
									? GetDropDownListColumnDefinitions(p.Metadata, p.DropDownList)
									: GetColumnDefinitions(p.Metadata, defaultValues, textAreas)
							)
					)
					.Union
					(
						hiddenValues?.Select(GetColumnDefinition) ?? Enumerable.Empty<JObject>()
					).ToArray()
			);
		}

		/// <summary>
		/// Returns the initial data for the table.
		/// </summary>
		private JArray GetInitialData(
			IDictionary<string, string> hiddenValues, 
			IEnumerable<object> collection,
			IList<DropDownList> dropDownLists,
			ModelMetadata elementMetadata,
			IEnumerable<string> initDataProperties)
		{
			JArray initData = null;
			if (collection != null)
			{
				initData = new JArray
				(
					collection.Select
					(
						obj => new JObject()
						{
							GetProperties(elementMetadata.Properties["Id"], obj)
								.Union
								(
									initDataProperties
										.Select(propName => elementMetadata.Properties[propName])
										.Select
										(
											propMetadata => new
											{
												Metadata = propMetadata,
												DropDownList = GetDropDownList(propMetadata, dropDownLists)
											}
										).SelectMany
										(
											p => p.DropDownList != null
												? GetDropDownListProperties(p.DropDownList, p.Metadata, obj)
												: GetProperties(p.Metadata, obj)
										)
								)
								.Union
								(
									hiddenValues?.Select(GetProperty) ?? Enumerable.Empty<JProperty>()
								)
						}
					).ToArray()
				);
			}

			return initData;
		}

		/// <summary>
		/// Returns the drop down list corresponding to the property, if it exists.
		/// </summary>
		private DropDownList GetDropDownList(
			ModelMetadata propertyMetadata, 
			IList<DropDownList> dropDownLists)
		{
			return dropDownLists?.SingleOrDefault
			(
				d => d.PropertyName == propertyMetadata.PropertyName
			);
		}

		/// <summary>
		/// Returns a collection, ordered by the property with the given name.
		/// </summary>
		private IEnumerable<object> GetOrderedCollection(ModelExplorer modelExplorer, string orderByProp)
		{
			var orderProp = modelExplorer.Metadata.ElementMetadata.Properties[orderByProp];
			var collection = (modelExplorer.Model as IEnumerable)?.Cast<object>();

			if (collection == null)
				return null;

			if (orderProp.ModelType == typeof(string))
				return collection.OrderBy(obj => (string) orderProp.PropertyGetter(obj));
			else if (orderProp.ModelType == typeof(int))
				return collection.OrderBy(obj => (int) orderProp.PropertyGetter(obj));
			else
				throw new InvalidOperationException("Unsupported order property type.");
		}

		/// <summary>
		/// Returns the definition for the ID column.
		/// </summary>
		private JObject GetIdColumnDefinition()
		{
			var columnInfo = new JObject();

			columnInfo["name"] = "Id";
			columnInfo["type"] = "hidden";
			columnInfo["value"] = 0;

			return columnInfo;
		}

		/// <summary>
		/// Returns the definition of the column (or columns) corresponding
		/// to the drop down list.
		/// </summary>
		private IEnumerable<JObject> GetDropDownListColumnDefinitions(
			ModelMetadata propMetadata,
			DropDownList dropDownList)
		{
			JObject groupColumnInfo = null;
			JObject objectColumnInfo = new JObject();

			objectColumnInfo["name"] = propMetadata.PropertyName;
			objectColumnInfo["display"] = propMetadata.DisplayName;
			objectColumnInfo["type"] = "select";

			if (propMetadata.IsRequired)
			{
				objectColumnInfo["ctrlClass"] = "required";
			}

			if (dropDownList.GroupFilter == null)
			{
				objectColumnInfo["ctrlOptions"] = "0:Select;" + string.Join
				(
					";",
					dropDownList.Choices
						.Select(dropDownList.ItemAccessor)
						.OrderBy(i => i.Name, new NaturalComparer())
						.Select(item => $"{item.Value}:{item.Name}")
				);

				yield return objectColumnInfo;
				yield break;
			}
			else
			{
				objectColumnInfo["ctrlAttr"] = new JObject()
				{
					["disabled"] = "disabled"
				};

				groupColumnInfo = new JObject();

				groupColumnInfo["name"] = dropDownList.GroupColumnName;
				groupColumnInfo["display"] = dropDownList.GroupColumnDisplayName;
				groupColumnInfo["type"] = "select";
				groupColumnInfo["ctrlOptions"] = "0:Select;" + string.Join
				(
					";",
					dropDownList.Choices
						.Select(dropDownList.GroupFilter)
						.Distinct()
						.OrderBy(i => i.Name, new NaturalComparer())
						.Select(item => $"{item.Value}:{item.Name}")
				);

				groupColumnInfo["onChange"] = new JRaw
				(
					$"handle{dropDownList.GroupColumnName}Changed"
				);

				yield return groupColumnInfo;
				yield return objectColumnInfo;
			}
		}

		/// <summary>
		/// Returns the definition of the column for a property.
		/// </summary>
		private IEnumerable<JObject> GetColumnDefinitions(
			ModelMetadata propMetadata,
			IDictionary<string, object> defaultValues,
			bool textArea)
		{
			var columnInfo = new JObject();

			columnInfo["name"] = propMetadata.PropertyName;
			columnInfo["display"] = propMetadata.DisplayName;
			columnInfo["type"] = GetColumnType(propMetadata, textArea);

			if (defaultValues?.ContainsKey(propMetadata.PropertyName) ?? false)
			{
				columnInfo["value"] = new JValue(defaultValues[propMetadata.PropertyName]);
			}

			if (propMetadata.IsRequired && propMetadata.ModelType != typeof(bool))
			{
				columnInfo["ctrlClass"] = "required";
			}

			if (propMetadata.ModelType == typeof(bool))
			{
				columnInfo["ctrlAttr"] = new JObject()
				{
					new JProperty("value", "true")
				};
			}

			if (propMetadata.ModelType.GetTypeInfo().IsEnum)
			{
				columnInfo["ctrlOptions"] = new JObject
				(
					propMetadata.EnumGroupedDisplayNamesAndValues.Select
					(
						choice => new JProperty(choice.Value, choice.Key.Name)
					)
				);
			}

			yield return columnInfo;
		}

		/// <summary>
		/// Returns the column type for the given property.
		/// </summary>
		private static string GetColumnType(ModelMetadata propMetadata, bool textArea)
		{
			if (propMetadata.ModelType == typeof(string))
				return textArea ? "textarea" : "text";
			else if (propMetadata.ModelType == typeof(int))
				return "text";
			else if (propMetadata.ModelType == typeof(double))
				return "text";
			else if (propMetadata.ModelType == typeof(DateTime) || propMetadata.ModelType == typeof(DateTime?))
				return "datetime-local";
			else if (propMetadata.ModelType == typeof(bool))
				return "checkbox";
			else if (propMetadata.ModelType.GetTypeInfo().IsEnum)
				return "select";
			else
				throw new InvalidOperationException("Unsupported property type.");
		}

		/// <summary>
		/// Returns the definition of a column for a hidden value.
		/// </summary>
		private JObject GetColumnDefinition(KeyValuePair<string, string> hiddenValue)
		{
			var columnInfo = new JObject();

			columnInfo["name"] = hiddenValue.Key;
			columnInfo["type"] = "hidden";
			columnInfo["value"] = hiddenValue.Value;

			return columnInfo;
		}

		/// <summary>
		/// Returns the properties for a drop down list.
		/// </summary>
		private IEnumerable<JProperty> GetDropDownListProperties(
			DropDownList dropDownList, 
			ModelMetadata modelMetadata, 
			object obj)
		{
			int value = (int)modelMetadata.PropertyGetter(obj);

			if (dropDownList.GroupFilter != null)
			{
				var choice = dropDownList.Choices
					.SingleOrDefault(c => dropDownList.ItemAccessor(c).Value == value);

				var groupValue = dropDownList.GroupFilter(choice).Value;

				yield return new JProperty(dropDownList.GroupColumnName, groupValue);
			}

			yield return new JProperty(dropDownList.PropertyName, value);
		}

		/// <summary>
		/// Returns a JSON property containing the value of a model property, for a given model object.
		/// </summary>
		private IEnumerable<JProperty> GetProperties(ModelMetadata modelMetadata, object obj)
		{
			object value = modelMetadata.PropertyGetter(obj);
			if (modelMetadata.ModelType == typeof(DateTime))
			{
				value = _timeZoneProvider.ToUserLocalTime((DateTime)value);
			}

			yield return new JProperty(modelMetadata.PropertyName, value);
		}

		/// <summary>
		/// Returns a JSON property containing a hidden value.
		/// </summary>
		private JProperty GetProperty(KeyValuePair<string, string> hiddenValue)
		{
			return new JProperty(hiddenValue.Key, hiddenValue.Value);
		}
	}
}
