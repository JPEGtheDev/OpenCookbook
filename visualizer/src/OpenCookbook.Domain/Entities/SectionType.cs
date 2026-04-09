namespace OpenCookbook.Domain.Entities;

public enum SectionType
{
    Sequence,
    Branch,

    /// <summary>
    /// Storage, freezing, or make-ahead instructions.
    /// Suppressed for sub-recipe sections when composed into a parent recipe.
    /// </summary>
    Storage
}
