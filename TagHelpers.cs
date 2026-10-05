using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TaskFlow.Models;

namespace TaskFlow;

/// <summary>&lt;icon name="trash" /&gt; → icône SVG du sprite (Pages/Shared/_IconSprite.cshtml).</summary>
[HtmlTargetElement("icon", TagStructure = TagStructure.WithoutEndTag)]
public class IconTagHelper : TagHelper
{
    public string Name { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        var extra = output.Attributes["class"]?.Value?.ToString();
        output.Attributes.SetAttribute("class", string.IsNullOrEmpty(extra) ? "bi" : $"bi {extra}");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Content.SetHtmlContent($"<use href=\"#i-{HtmlEncoder.Default.Encode(Name)}\"></use>");
    }
}

/// <summary>&lt;avatar user-id="..." name="Camille Martin" /&gt; → pastille colorée avec les initiales.</summary>
[HtmlTargetElement("avatar", TagStructure = TagStructure.WithoutEndTag)]
public class AvatarTagHelper : TagHelper
{
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        var extra = output.Attributes["class"]?.Value?.ToString();
        output.Attributes.SetAttribute("class", $"avatar tone-{UserDisplay.Tone(UserId)} {extra}".Trim());
        output.Attributes.SetAttribute("title", Name);
        output.Content.SetContent(UserDisplay.Initials(Name));
    }
}

/// <summary>&lt;tag-chip value="docker" /&gt; → étiquette colorée.</summary>
[HtmlTargetElement("tag-chip", TagStructure = TagStructure.WithoutEndTag)]
public class TagChipTagHelper : TagHelper
{
    public string Value { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"tag-chip tone-{UserDisplay.Tone(Value)}");
        output.Content.SetContent($"#{Value}");
    }
}
