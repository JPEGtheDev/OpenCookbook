using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Interfaces;

public interface INutritionRepository
{
    Task<IReadOnlyList<NutritionEntry>> GetAllEntriesAsync();
}
