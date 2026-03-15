using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

internal sealed class FakeNutritionRepository : INutritionRepository
{
    private readonly IReadOnlyList<NutritionEntry> _entries;

    public FakeNutritionRepository(IReadOnlyList<NutritionEntry> entries)
    {
        _entries = entries;
    }

    public Task<IReadOnlyList<NutritionEntry>> GetAllEntriesAsync()
    {
        return Task.FromResult(_entries);
    }
}
