namespace OpenCookbook.Domain.Entities;

public enum SectionType
{
    Sequence,
    Branch,

    /// <summary>
    /// Storage, freezing, or make-ahead instructions.
    /// Suppressed when a sub-recipe is composed into a parent recipe.
    /// </summary>
    Storage
}
