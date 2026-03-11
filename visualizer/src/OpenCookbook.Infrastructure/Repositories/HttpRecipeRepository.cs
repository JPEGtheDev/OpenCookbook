using System.Net.Http.Json;
using System.Text.Json;
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
        PropertyNameCaseInsensitive = true
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

        var yamlContent = await _httpClient.GetStringAsync($"recipes/{path}");

        return _parser.Parse(yamlContent);
    }
}
