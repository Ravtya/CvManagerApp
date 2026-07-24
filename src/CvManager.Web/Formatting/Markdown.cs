using Markdig;

namespace CvManager.Web.Formatting;

public static class Markdown
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseAdvancedExtensions()
        .Build();

    public static string ToHtml(string? markdown) =>
        global::Markdig.Markdown.ToHtml(markdown ?? string.Empty, Pipeline);
}
