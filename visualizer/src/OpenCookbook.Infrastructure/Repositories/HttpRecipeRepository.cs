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

    public async Task<Recipe> GetRecipeFromUrlAsync(string absoluteUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absoluteUrl);

        if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Recipe URL must use HTTPS.", nameof(absoluteUrl));

        if (!absoluteUrl.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Recipe URL must point to a .yaml file.", nameof(absoluteUrl));

        var yamlContent = await _httpClient.GetStringAsync(absoluteUrl);
        return _parser.Parse(yamlContent);
    }

    private static string SanitizeRecipePath(string path)
    {
        if (path.StartsWith('/') || path.StartsWith('\\'))
            throw new ArgumentException("Recipe path must not start with a slash.", nameof(path));

        if (!path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Recipe path must have a .yaml extension.", nameof(path));

        // Normalize and check for directory traversal via resolved path
        var normalized = System.IO.Path.GetFullPath(System.IO.Path.Combine("recipes", path));
        var basePath = System.IO.Path.GetFullPath("recipes");
        if (!normalized.StartsWith(basePath + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && normalized != basePath)
            throw new ArgumentException("Recipe path must not escape the recipes directory.", nameof(path));

        return path;
    }
}
