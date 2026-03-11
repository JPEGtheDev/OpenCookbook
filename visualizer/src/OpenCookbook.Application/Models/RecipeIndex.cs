using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Models;

public class RecipeIndex
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public RecipeStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
}
