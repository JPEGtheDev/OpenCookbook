namespace OpenCookbook.Domain.Entities;

public class Step
{
    public string Text { get; set; } = string.Empty;
    public List<string>? Notes { get; set; }
}
