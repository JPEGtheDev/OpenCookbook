using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Infrastructure.Repositories;

public sealed class HttpRecipeRepository : IRecipeRepository
{
    private readonly HttpClient _httpClient;
    private readonly IRecipeParser _parser;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public HttpRecipeRepository(HttpClient httpClient, IRecipeParser parser)
    {
        _httpClient = httpClient;
        _parser = parser;
    }

    public async Task<IReadOnlyList<RecipeIndex>> GetRecipeIndexAsync()
    {
        var recipes = await _httpClient.GetFromJsonAsync<List<RecipeIndex>>(
            "recipes/recipe-index.json", JsonOptions);

        return recipes ?? [];
    }

    public async Task<Recipe> GetRecipeAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var sanitized = SanitizeRecipePath(path);
        var yamlContent = await _httpClient.GetStringAsync($"recipes/{sanitized}");

        return _parser.Parse(yamlContent);
    }

    private static string SanitizeRecipePath(string path)
    {
        if (path.StartsWith('/') || path.StartsWith('\\'))
            throw new ArgumentException("Recipe path must not start with a slash.", nameof(path));

        // Trim trailing separators before computing the extension so that paths like
        // "Recipes/Beta/Shrimp_Scampi/" don't become "Recipes/Beta/Shrimp_Scampi/.yaml".
        path = path.TrimEnd('/', '\\');

        // Normalise: append .yaml extension if the caller omitted it (canonical slug support).
        // Only add it when the path has no extension at all; reject paths with a different extension.
        var ext = System.IO.Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext))
        {
            path += ".yaml";
        }
        else if (!ext.Equals(".yaml", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Recipe path has an unsupported extension '{ext}'. Only .yaml is accepted.",
                nameof(path));
        }

        // Normalize separators to URL-style forward slashes before returning so callers
        // passing decoded route values with backslashes get a valid HTTP path segment.
        path = path.Replace('\\', '/');

        // Check for directory traversal via resolved path
        var normalized = System.IO.Path.GetFullPath(System.IO.Path.Combine("recipes", path));
        var basePath = System.IO.Path.GetFullPath("recipes");
        if (!normalized.StartsWith(basePath + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && normalized != basePath)
            throw new ArgumentException("Recipe path must not escape the recipes directory.", nameof(path));

        return path;
    }
}
