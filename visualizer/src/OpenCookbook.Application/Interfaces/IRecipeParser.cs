using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Interfaces;

public interface IRecipeParser
{
    Recipe Parse(string yamlContent);
}
