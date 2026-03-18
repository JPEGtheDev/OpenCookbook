namespace OpenCookbook.Application.Services;

/// <summary>
/// Shared utilities for resolving <c>doc_link</c> paths across services.
/// Normalises relative paths (<c>./</c>, <c>..</c>) so the result can be
/// passed directly to <see cref="Interfaces.IRecipeRepository.GetRecipeAsync"/>.
/// </summary>
public static class DocLinkResolver
{
    /// <summary>
    /// Resolves a relative <paramref name="docLink"/> against the
    /// <paramref name="basePath"/> directory and normalises the result.
    /// </summary>
    public static string ResolvePath(string? basePath, string docLink)
    {
        var linkPath = docLink.StartsWith("./", StringComparison.Ordinal) ? docLink[2..] : docLink;

        var combined = string.IsNullOrEmpty(basePath)
            ? linkPath
            : $"{basePath}/{linkPath}";

        var parts = combined.Split('/');
        var normalized = new List<string>();
        foreach (var part in parts)
        {
            if (part == "..")
            {
                if (normalized.Count > 0)
                    normalized.RemoveAt(normalized.Count - 1);
            }
            else if (part != "." && part.Length > 0)
            {
                normalized.Add(part);
            }
        }

        return string.Join("/", normalized);
    }

    /// <summary>
    /// Returns the directory portion of a recipe path, or <c>null</c>
    /// if the path has no directory component.
    /// </summary>
    public static string? GetDirectory(string? path)
    {
        if (path is null) return null;
        var lastSlash = path.LastIndexOf('/');
        return lastSlash > 0 ? path[..lastSlash] : null;
    }
}
