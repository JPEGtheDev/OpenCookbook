namespace OpenCookbook.Web.Pages;

internal static class RecipeUrlHelper
{
    /// <summary>
    /// Encodes a recipe file path for use in a canonical URL, preserving '/' path separators
    /// and stripping the <c>.yaml</c> extension if present.
    /// Each path segment is percent-encoded individually so slashes remain as real '/' characters.
    /// </summary>
    public static string EscapeRecipePath(string path)
    {
        var canonical = path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
            ? path[..^5]
            : path;
        return string.Join("/", canonical.Split('/').Select(Uri.EscapeDataString));
    }
}
