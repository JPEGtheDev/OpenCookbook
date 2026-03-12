using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenCookbook.Infrastructure.Parsing;

public sealed class YamlRecipeParser : IRecipeParser
{
    private readonly IDeserializer _deserializer;

    public YamlRecipeParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeConverter(new RecipeStatusYamlConverter())
            .WithTypeConverter(new SectionTypeYamlConverter())
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public Recipe Parse(string yamlContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yamlContent);

        try
        {
            return _deserializer.Deserialize<Recipe>(yamlContent);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse recipe YAML: {ex.Message}", ex);
        }
    }
}
