using System.Text.Encodings.Web;
using HxhGuide.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HxhGuide;

/// <summary>&lt;icon name="trophy" /&gt; → icône SVG du sprite (Pages/Shared/_IconSprite.cshtml).</summary>
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

/// <summary>&lt;avatar name="Killua Zoldyck" nen="Transformation" /&gt; → initiales sur la couleur du Nen.</summary>
[HtmlTargetElement("avatar", TagStructure = TagStructure.WithoutEndTag)]
public class AvatarTagHelper : TagHelper
{
    public string Name { get; set; } = "";
    public NenType? Nen { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        var extra = output.Attributes["class"]?.Value?.ToString();
        var nen = Nen is { } type ? $"nen-{type.Info().Slug}" : "nen-none";
        output.Attributes.SetAttribute("class", $"avatar {nen} {extra}".Trim());
        output.Attributes.SetAttribute("title", Nen is { } t ? $"{Name} ({t.Info().Name})" : Name);
        output.Content.SetContent(Initials.Of(Name));
    }
}

/// <summary>
/// &lt;portrait character="@c" class="avatar-lg" /&gt; → image du personnage si elle existe
/// (wwwroot/images/personnages), sinon avatar à initiales. Toujours entouré de la couleur de son Nen.
/// </summary>
[HtmlTargetElement("portrait", TagStructure = TagStructure.WithoutEndTag)]
public class PortraitTagHelper(CharacterImages images) : TagHelper
{
    public Character Character { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var extra = output.Attributes["class"]?.Value?.ToString();
        var nen = Character.Nen is { } type ? $"nen-{type.Info().Slug}" : "nen-none";
        var url = images.UrlFor(Character.Slug);
        output.Attributes.SetAttribute("class", $"avatar {nen} {(url is null ? "" : "portrait")} {extra}".Trim().Replace("  ", " "));
        output.Attributes.SetAttribute("title", Character.Name);

        if (url is not null)
        {
            output.TagName = "img";
            output.TagMode = TagMode.SelfClosing;
            output.Attributes.SetAttribute("src", url);
            output.Attributes.SetAttribute("alt", $"Portrait de {Character.Name}");
            output.Attributes.SetAttribute("loading", "lazy");
            output.Attributes.SetAttribute("decoding", "async");
        }
        else
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetContent(Initials.Of(Character.Name));
        }
    }
}

/// <summary>&lt;nen-chip type="..." /&gt; → pastille colorée avec le nom du type de Nen.</summary>
[HtmlTargetElement("nen-chip", TagStructure = TagStructure.WithoutEndTag)]
public class NenChipTagHelper : TagHelper
{
    public NenType? Type { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        var extra = output.Attributes["class"]?.Value?.ToString();
        if (Type is { } type)
        {
            output.Attributes.SetAttribute("class", $"nen-chip nen-{type.Info().Slug} {extra}".Trim());
            output.Content.SetContent(type.Info().Name);
        }
        else
        {
            output.Attributes.SetAttribute("class", $"nen-chip nen-none {extra}".Trim());
            output.Content.SetContent("Nen non précisé");
        }
    }
}

/// <summary>&lt;stars count="2" /&gt; → étoiles dorées (difficulté ou rang).</summary>
[HtmlTargetElement("stars", TagStructure = TagStructure.WithoutEndTag)]
public class StarsTagHelper : TagHelper
{
    public int Count { get; set; }
    public int Max { get; set; } = 3;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        var extra = output.Attributes["class"]?.Value?.ToString();
        output.Attributes.SetAttribute("class", $"stars {extra}".Trim());
        output.Attributes.SetAttribute("role", "img");
        output.Attributes.SetAttribute("aria-label", Count switch { 0 => "Aucune étoile", 1 => "1 étoile", _ => $"{Count} étoiles" });
        var html = string.Concat(Enumerable.Range(1, Max).Select(i =>
            i <= Count
                ? "<svg class=\"bi star-on\" aria-hidden=\"true\"><use href=\"#i-star-fill\"></use></svg>"
                : "<svg class=\"bi star-off\" aria-hidden=\"true\"><use href=\"#i-star\"></use></svg>"));
        output.Content.SetHtmlContent(html);
    }
}

/// <summary>&lt;tag-chip value="capture" /&gt; → catégorie de mission.</summary>
[HtmlTargetElement("tag-chip", TagStructure = TagStructure.WithoutEndTag)]
public class TagChipTagHelper : TagHelper
{
    public string Value { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "tag-chip");
        output.Content.SetContent($"#{Value}");
    }
}
