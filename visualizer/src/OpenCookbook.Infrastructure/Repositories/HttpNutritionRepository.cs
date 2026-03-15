using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Infrastructure.Repositories;

public sealed class HttpNutritionRepository : INutritionRepository
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        Converters = { new NutrientInfoJsonConverter() }
    };

    public HttpNutritionRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<NutritionEntry>> GetAllEntriesAsync()
    {
        try
        {
            var entries = await _httpClient.GetFromJsonAsync<List<NutritionEntry>>(
                "data/nutrition-db.json", JsonOptions);

            return entries ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            Console.Error.WriteLine($"Failed to load nutrition database: {ex.Message}");
            return [];
        }
    }

    private sealed class NutrientInfoJsonConverter : JsonConverter<NutrientInfo>
    {
        public override NutrientInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject");

            var info = new NutrientInfo();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return info;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected PropertyName");

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "calories_kcal":
                        info.CaloriesKcal = reader.GetDouble();
                        break;
                    case "protein_g":
                        info.ProteinG = reader.GetDouble();
                        break;
                    case "fat_g":
                        info.FatG = reader.GetDouble();
                        break;
                    case "carbs_g":
                        info.CarbsG = reader.GetDouble();
                        break;
                }
            }

            return info;
        }

        public override void Write(Utf8JsonWriter writer, NutrientInfo value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("calories_kcal", value.CaloriesKcal);
            writer.WriteNumber("protein_g", value.ProteinG);
            writer.WriteNumber("fat_g", value.FatG);
            writer.WriteNumber("carbs_g", value.CarbsG);
            writer.WriteEndObject();
        }
    }
}
