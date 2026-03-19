namespace OpenCookbook.Domain.Entities;

public class Section
{
    public string? Heading { get; set; }
    public SectionType Type { get; set; } = SectionType.Sequence;
    public string? BranchGroup { get; set; }
    public bool Optional { get; set; }
    public List<Step> Steps { get; set; } = [];

    /// <summary>
    /// When set, the composer resolves this path to a sub-recipe and inserts
    /// its instructions at this position in the instruction list.
    /// </summary>
    public string? DocLink { get; set; }
}
