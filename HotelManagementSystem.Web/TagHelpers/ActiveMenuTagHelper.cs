using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HotelManagementSystem.Web.TagHelpers;

[HtmlTargetElement("a", Attributes = "active-menu")]
public sealed class ActiveMenuTagHelper : TagHelper
{
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(
        TagHelperContext context,
        TagHelperOutput output)
    {
        var currentController = ViewContext.RouteData.Values["controller"]
            ?.ToString();

        var currentAction = ViewContext.RouteData.Values["action"]
            ?.ToString();

        var linkController =
            context.AllAttributes["asp-controller"]?.Value?.ToString()
            ?? currentController;

        var linkAction =
            context.AllAttributes["asp-action"]?.Value?.ToString();

        var isActive =
            string.Equals(
                linkController,
                currentController,
                StringComparison.OrdinalIgnoreCase) &&
            (linkAction is null ||
             string.Equals(
                 linkAction,
                 currentAction,
                 StringComparison.OrdinalIgnoreCase));

        if (isActive)
        {
            var existingClass =
                output.Attributes["class"]?.Value?.ToString() ?? string.Empty;

            output.Attributes.SetAttribute(
                "class",
                $"{existingClass} active".Trim());
        }

        output.Attributes.RemoveAll("active-menu");
    }
}