using Microsoft.Extensions.DependencyInjection;
using OpenCookbook.Application.Interfaces;
using OpenCookbook.Infrastructure.Parsing;
using OpenCookbook.Infrastructure.Repositories;

namespace OpenCookbook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRecipeParser, YamlRecipeParser>();
        services.AddScoped<IRecipeRepository, HttpRecipeRepository>();
        services.AddScoped<INutritionRepository, HttpNutritionRepository>();

        return services;
    }
}
