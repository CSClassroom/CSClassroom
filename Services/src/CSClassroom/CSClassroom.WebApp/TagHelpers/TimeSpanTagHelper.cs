using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CSC.CSClassroom.WebApp.TagHelpers
{
	/// <summary>
	/// Renders a time in the user's local time zone
	/// Derived from: https://github.com/aspnet/Mvc/issues/4871
	/// </summary>
	[HtmlTargetElement("input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
	public class TimeSpanTagHelper : TagHelper
	{
		private const string ForAttributeName = "asp-for";

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
			if (fieldType == typeof(TimeSpan) || fieldType == typeof(TimeSpan?))
			{
				var localTime = For.ModelExplorer.Model as TimeSpan?;

				if (localTime != null)
				{
					output.Attributes.SetAttribute
					(
						"value",
						localTime.Value.Seconds > 0
							? localTime.Value.ToString("hh\\:mm\\:ss")
							: localTime.Value.ToString("hh\\:mm")
					);
				}

				if (string.IsNullOrEmpty(modelExplorer.Metadata.TemplateHint) 
					&& string.IsNullOrEmpty(modelExplorer.Metadata.DataTypeName)
					&& (output.Attributes["type"] == null 
						|| (string)output.Attributes["type"].Value == "text"))
				{
					output.Attributes.SetAttribute("type", "time");
				}
			}
		}
	}
}