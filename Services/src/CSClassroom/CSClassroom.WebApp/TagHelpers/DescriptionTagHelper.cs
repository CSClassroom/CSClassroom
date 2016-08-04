using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CSC.CSClassroom.WebApp.TagHelpers
{
	/// <summary>
	/// Outputs the description of a model property. 
	/// Adapted from LabelTagHelper.cs in ASP.NET Core MVC.
	/// </summary>
	[HtmlTargetElement("p", Attributes = ForAttributeName)]
	public class DescriptionTagHelper : TagHelper
	{
		private const string ForAttributeName = "asp-description-for";

		/// <summary>
		/// Constructor.
		/// </summary>
		public DescriptionTagHelper(IHtmlGenerator generator)
		{
			Generator = generator;
		}

		/// <summary>
		/// The order of the tag helper.
		/// </summary>
		public override int Order
		{
			get
			{
				return -1000;
			}
		}

		/// <summary>
		/// The view context.
		/// </summary>
		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		/// <summary>
		/// The HTML generator.
		/// </summary>
		protected IHtmlGenerator Generator { get; }

		/// <summary>
		/// An expression to be evaluated against the current model.
		/// </summary>
		[HtmlAttributeName(ForAttributeName)]
		public ModelExpression For { get; set; }

		/// <summary>
		/// Applies the tag helper.
		/// </summary>
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (output == null)
			{
				throw new ArgumentNullException(nameof(output));
			}

			var tagBuilder = Generator.GenerateLabel(
				ViewContext,
				For.ModelExplorer,
				For.Name,
				labelText: For.ModelExplorer.Metadata.Description,
				htmlAttributes: null);

			if (tagBuilder != null)
			{
				output.MergeAttributes(tagBuilder);

				// We check for whitespace to detect scenarios such as:
				// <label asp-description-for="Name">
				// </label>
				if (!output.IsContentModified)
				{
					var childContent = await output.GetChildContentAsync();

					if (childContent.IsEmptyOrWhiteSpace)
					{
						// Provide default label text since there was nothing useful in the Razor source.
						output.Content.SetHtmlContent(tagBuilder.InnerHtml);
					}
					else
					{
						output.Content.SetHtmlContent(childContent);
					}
				}
			}
		}
	}
}