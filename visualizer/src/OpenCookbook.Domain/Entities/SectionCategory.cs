namespace OpenCookbook.Domain.Entities;

/// <summary>
/// Classifies the purpose of an instruction section.
/// Orthogonal to <see cref="SectionType"/> which describes the flow (sequence vs. branch).
/// </summary>
public enum SectionCategory
{
    /// <summary>
    /// Storage, freezing, or make-ahead instructions.
    /// Suppressed when a sub-recipe is composed into a parent recipe.
    /// </summary>
    Storage
}
