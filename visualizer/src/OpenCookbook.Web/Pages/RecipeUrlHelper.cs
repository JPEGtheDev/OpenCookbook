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
        var normalizedPath = path.Replace('\\', '/').TrimEnd('/');
        var canonical = normalizedPath.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
            ? normalizedPath[..^5]
            : normalizedPath;
        return string.Join("/", canonical.Split('/').Select(Uri.EscapeDataString));
    }
}
