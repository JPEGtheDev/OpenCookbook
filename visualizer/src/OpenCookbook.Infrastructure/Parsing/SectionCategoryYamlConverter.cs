using OpenCookbook.Domain.Entities;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OpenCookbook.Infrastructure.Parsing;

internal sealed class SectionCategoryYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(SectionCategory);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();

        if (Enum.TryParse<SectionCategory>(scalar.Value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new YamlException(
            scalar.Start, scalar.End,
            $"Invalid section category '{scalar.Value}'. Expected one of: storage.");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var category = (SectionCategory)value!;
        emitter.Emit(new Scalar(category.ToString().ToLowerInvariant()));
    }
}
