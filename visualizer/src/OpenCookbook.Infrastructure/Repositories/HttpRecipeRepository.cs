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

        if (path.Contains(".."))
            throw new ArgumentException("Recipe path must not contain directory traversal sequences.", nameof(path));

        if (!path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Recipe path must have a .yaml extension.", nameof(path));

        return path;
    }
}
