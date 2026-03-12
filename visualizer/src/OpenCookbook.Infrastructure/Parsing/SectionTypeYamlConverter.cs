using OpenCookbook.Domain.Entities;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OpenCookbook.Infrastructure.Parsing;

internal sealed class SectionTypeYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(SectionType);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();

        if (Enum.TryParse<SectionType>(scalar.Value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new YamlException(
            scalar.Start, scalar.End,
            $"Invalid section type '{scalar.Value}'. Expected one of: sequence, branch.");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var sectionType = (SectionType)value!;
        emitter.Emit(new Scalar(sectionType.ToString().ToLowerInvariant()));
    }
}
