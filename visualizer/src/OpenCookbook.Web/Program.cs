using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpenCookbook.Web;
using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Services;
using OpenCookbook.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddInfrastructure();
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<FitnessExportService>(sp =>
    new FitnessExportService(sp.GetRequiredService<IRecipeRepository>()));
builder.Services.AddScoped<NutritionCalculator>(sp =>
    new NutritionCalculator(
        sp.GetRequiredService<INutritionRepository>(),
        sp.GetRequiredService<IRecipeRepository>()));

await builder.Build().RunAsync();
