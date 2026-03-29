namespace OpenCookbook.Web.Pages;

internal static class RecipeUrlHelper
{
    /// <summary>
    /// Encodes a recipe file path for use in a URL, preserving '/' path separators.
    /// Each path segment is percent-encoded individually so slashes remain as real '/' characters.
    /// </summary>
    public static string EscapeRecipePath(string path) =>
        string.Join("/", path.Split('/').Select(Uri.EscapeDataString));
}
