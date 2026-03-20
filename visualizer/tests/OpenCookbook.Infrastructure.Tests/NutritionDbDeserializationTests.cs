using System.Net;
using OpenCookbook.Infrastructure.Repositories;

namespace OpenCookbook.Infrastructure.Tests;

public class NutritionDbDeserializationTests
{
    private static string FindNutritionDbJsonPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !dir.GetFiles("OpenCookbook.slnx").Any())
        {
            dir = dir.Parent;
        }

        // The solution root is visualizer/; the source-of-truth lives one level up in docs/data/
        var path = dir?.Parent is not null
            ? Path.Combine(dir.Parent.FullName, "docs", "data", "nutrition-db.json")
            : throw new FileNotFoundException("Could not find repo root to locate nutrition-db.json");

        return path;
    }

    private static async Task<HttpNutritionRepository> CreateRepositoryFromRealJsonAsync()
    {
        var json = await File.ReadAllTextAsync(FindNutritionDbJsonPath(), TestContext.Current.CancellationToken);
        var handler = new FakeHttpMessageHandler(json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") };
        return new HttpNutritionRepository(httpClient);
    }

    [Fact]
    public async Task GetAllEntriesAsync_RealJson_DeserializesAllEntries()
    {
        // Arrange
        var repository = await CreateRepositoryFromRealJsonAsync();

        // Act
        var entries = await repository.GetAllEntriesAsync();

        // Assert
        Assert.Equal(36, entries.Count);
    }

    [Fact]
    public async Task GetAllEntriesAsync_RealJson_BrownSugarHasCalories()
    {
        // Arrange
        var repository = await CreateRepositoryFromRealJsonAsync();

        // Act
        var entries = await repository.GetAllEntriesAsync();

        // Assert
        var brownSugar = entries.First(e => e.Name == "Brown Sugar");
        Assert.Equal(380, brownSugar.Per100g.CaloriesKcal);
        Assert.Equal(0.1, brownSugar.Per100g.ProteinG);
        Assert.Equal(0.0, brownSugar.Per100g.FatG);
        Assert.Equal(98.1, brownSugar.Per100g.CarbsG);
    }

    [Fact]
    public async Task GetAllEntriesAsync_RealJson_AllEntriesWithCaloriesHaveNonZeroPer100g()
    {
        // Arrange
        var repository = await CreateRepositoryFromRealJsonAsync();

        // Act
        var entries = await repository.GetAllEntriesAsync();

        // Assert — zero-calorie entries (salt, water, hot sauce) are allowed
        var zeroCalorieNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Fine Sea Salt", "Water", "Franks Red Hot"
        };

        foreach (var entry in entries)
        {
            if (zeroCalorieNames.Contains(entry.Name))
                continue;

            Assert.True(
                entry.Per100g.CaloriesKcal > 0,
                $"{entry.Name} (id={entry.Id}) has 0 CaloriesKcal after deserialization");
        }
    }

    [Fact]
    public async Task GetAllEntriesAsync_RealJson_EntriesHaveIds()
    {
        // Arrange
        var repository = await CreateRepositoryFromRealJsonAsync();

        // Act
        var entries = await repository.GetAllEntriesAsync();

        // Assert
        foreach (var entry in entries)
        {
            Assert.NotEqual(Guid.Empty, entry.Id);
        }
    }

    [Fact]
    public async Task GetAllEntriesAsync_RealJson_GuajilloChilesHasNutrients()
    {
        // Arrange
        var repository = await CreateRepositoryFromRealJsonAsync();

        // Act
        var entries = await repository.GetAllEntriesAsync();

        // Assert
        var guajillo = entries.First(e => e.Name == "Guajillo Chiles");
        Assert.Equal(314, guajillo.Per100g.CaloriesKcal);
        Assert.Equal(11.0, guajillo.Per100g.ProteinG);
        Assert.Equal(5.8, guajillo.Per100g.FatG);
        Assert.Equal(56.0, guajillo.Per100g.CarbsG);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;

        public FakeHttpMessageHandler(string responseContent)
        {
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
