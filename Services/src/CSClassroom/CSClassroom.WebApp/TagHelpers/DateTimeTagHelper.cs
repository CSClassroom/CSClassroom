using System;
using CSC.CSClassroom.WebApp.Providers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CSC.CSClassroom.WebApp.TagHelpers
{
	/// <summary>
	/// Renders a date time in the user's local time zone
	/// Derived from: https://github.com/aspnet/Mvc/issues/4871
	/// </summary>
	[HtmlTargetElement("input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
	public class DateTimeTagHelper : TagHelper
	{
		private const string ForAttributeName = "asp-for";

		/// <summary>
		/// The time zone provider.
		/// </summary>
		private readonly ITimeZoneProvider _timeZoneProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DateTimeTagHelper(ITimeZoneProvider timeZoneProvider)
		{
			_timeZoneProvider = timeZoneProvider;
		}

		/// <summary>
		/// The order of the tag helper. The default order of the built-in
		/// TagHelpers is -1000. By being one higher, this will run after them.
		/// </summary>
		public override int Order { get; } = -999;

		/// <summary>
		/// An expression to be evaluated against the current model.
		/// </summary>
		[HtmlAttributeName(ForAttributeName)]
		public ModelExpression For { get; set; }

		/// <summary>
		/// Applies the tag helper.
		/// </summary>
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var modelExplorer = For.ModelExplorer;
			var fieldType = modelExplorer.Metadata.UnderlyingOrModelType;
			if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?))
			{
				var localTime = _timeZoneProvider.ToUserLocalTime((DateTime) For.Model);
				output.Attributes.SetAttribute
				(
					"value",
					localTime.Second > 0
						? localTime.ToString("s")
						: localTime.ToString("yyyy-MM-ddTHH\\:mm")
				);

				if (string.IsNullOrEmpty(modelExplorer.Metadata.TemplateHint) 
					&& string.IsNullOrEmpty(modelExplorer.Metadata.DataTypeName)
					&& (output.Attributes["type"] == null
						|| (string)output.Attributes["type"].Value == "datetime"))
				{
					output.Attributes.SetAttribute("type", "datetime-local");
				}
			}
		}
	}
}
