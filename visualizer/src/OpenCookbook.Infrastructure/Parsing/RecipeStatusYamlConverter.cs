using OpenCookbook.Domain.Entities;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OpenCookbook.Infrastructure.Parsing;

internal sealed class RecipeStatusYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(RecipeStatus);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();

        if (Enum.TryParse<RecipeStatus>(scalar.Value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new YamlException(
            scalar.Start, scalar.End,
            $"Invalid recipe status '{scalar.Value}'. Expected one of: stable, beta, draft.");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var status = (RecipeStatus)value!;
        emitter.Emit(new Scalar(status.ToString().ToLowerInvariant()));
    }
}
