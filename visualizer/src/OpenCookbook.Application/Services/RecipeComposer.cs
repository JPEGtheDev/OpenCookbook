using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

/// <summary>
/// Recursively composes a recipe by resolving all <see cref="Ingredient.DocLink"/>
/// references and merging sub-recipe ingredients, instructions, and utensils into
/// the parent recipe. Detects cycles to prevent infinite recursion.
/// </summary>
public class RecipeComposer
{
    private readonly IRecipeRepository _recipeRepository;

    public RecipeComposer(IRecipeRepository recipeRepository)
    {
        _recipeRepository = recipeRepository;
    }

    /// <summary>
    /// Returns a new <see cref="Recipe"/> with all doc_link ingredients resolved
    /// and their contents merged inline. The original recipe is not mutated.
    /// </summary>
    /// <param name="recipe">The root recipe to compose.</param>
    /// <param name="recipePath">
    /// The path of <paramref name="recipe"/> relative to the recipe root,
    /// used to resolve relative doc_link paths.
    /// </param>
    public async Task<Recipe> ComposeAsync(Recipe recipe, string? recipePath)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (recipePath is not null)
            visited.Add(recipePath);

        return await ComposeRecursiveAsync(recipe, GetDirectoryFromPath(recipePath), visited);
    }

    private async Task<Recipe> ComposeRecursiveAsync(
        Recipe recipe,
        string? basePath,
        HashSet<string> visited)
    {
        var composedIngredients = new List<IngredientGroup>();
        var prependedInstructions = new List<Section>();
        var mergedUtensils = new List<UtensilGroup>(recipe.Utensils ?? []);

        foreach (var group in recipe.Ingredients)
        {
            var hasDocLink = false;
            foreach (var item in group.Items)
            {
                if (item.DocLink is not null)
                {
                    hasDocLink = true;
                    break;
                }
            }

            if (!hasDocLink)
            {
                composedIngredients.Add(group);
                continue;
            }

            // Process group items, replacing doc_link items with sub-recipe contents
            var remainingItems = new List<Ingredient>();

            foreach (var item in group.Items)
            {
                if (item.DocLink is null)
                {
                    remainingItems.Add(item);
                    continue;
                }

                // Flush any accumulated non-doc-link items into a group
                if (remainingItems.Count > 0)
                {
                    composedIngredients.Add(new IngredientGroup
                    {
                        Heading = group.Heading,
                        Items = remainingItems
                    });
                    remainingItems = [];
                }

                var resolvedPath = ResolveSubRecipePath(basePath, item.DocLink);

                // Cycle detection: skip if already visited
                if (!visited.Add(resolvedPath))
                    continue;

                Recipe subRecipe;
                try
                {
                    subRecipe = await _recipeRepository.GetRecipeAsync(resolvedPath);
                }
                catch
                {
                    // If sub-recipe fails to load, keep the original ingredient as-is
                    visited.Remove(resolvedPath);
                    remainingItems.Add(item);
                    continue;
                }

                // Recursively compose the sub-recipe first
                var subBasePath = GetDirectoryFromPath(resolvedPath);
                subRecipe = await ComposeRecursiveAsync(subRecipe, subBasePath, visited);

                // Merge sub-recipe ingredients under a heading
                foreach (var subGroup in subRecipe.Ingredients)
                {
                    composedIngredients.Add(new IngredientGroup
                    {
                        Heading = subGroup.Heading ?? subRecipe.Name,
                        Items = subGroup.Items
                    });
                }

                // Prepend sub-recipe instructions (they must be done first)
                foreach (var instrSection in subRecipe.Instructions)
                {
                    prependedInstructions.Add(new Section
                    {
                        Heading = instrSection.Heading ?? subRecipe.Name,
                        Type = instrSection.Type,
                        BranchGroup = instrSection.BranchGroup,
                        Optional = instrSection.Optional,
                        Steps = instrSection.Steps
                    });
                }

                // Merge utensils (deduplicate)
                MergeUtensils(mergedUtensils, subRecipe.Utensils);
            }

            // Flush any remaining items
            if (remainingItems.Count > 0)
            {
                composedIngredients.Add(new IngredientGroup
                {
                    Heading = group.Heading,
                    Items = remainingItems
                });
            }
        }

        // Build composed instruction list: sub-recipe instructions first, then parent.
        // Resolve any instruction-level doc_link sections inline.
        var composedInstructions = new List<Section>(prependedInstructions.Count + recipe.Instructions.Count);
        composedInstructions.AddRange(prependedInstructions);

        var hasSubRecipeInstructions = prependedInstructions.Count > 0;

        foreach (var instrSection in recipe.Instructions)
        {
            if (instrSection.DocLink is not null)
            {
                var resolvedPath = ResolveSubRecipePath(basePath, instrSection.DocLink);

                if (visited.Add(resolvedPath))
                {
                    Recipe? linkedRecipe = null;
                    try
                    {
                        linkedRecipe = await _recipeRepository.GetRecipeAsync(resolvedPath);
                    }
                    catch
                    {
                        visited.Remove(resolvedPath);
                    }

                    if (linkedRecipe is not null)
                    {
                        var subBasePath = GetDirectoryFromPath(resolvedPath);
                        linkedRecipe = await ComposeRecursiveAsync(linkedRecipe, subBasePath, visited);

                        // Merge ingredients from instruction-level doc_link
                        foreach (var subGroup in linkedRecipe.Ingredients)
                        {
                            composedIngredients.Add(new IngredientGroup
                            {
                                Heading = subGroup.Heading ?? linkedRecipe.Name,
                                Items = subGroup.Items
                            });
                        }

                        // Insert the sub-recipe instructions at this position
                        foreach (var subInstr in linkedRecipe.Instructions)
                        {
                            composedInstructions.Add(new Section
                            {
                                Heading = subInstr.Heading ?? linkedRecipe.Name,
                                Type = subInstr.Type,
                                BranchGroup = subInstr.BranchGroup,
                                Optional = subInstr.Optional,
                                Steps = subInstr.Steps
                            });
                        }

                        MergeUtensils(mergedUtensils, linkedRecipe.Utensils);
                        hasSubRecipeInstructions = true;
                    }
                }

                // doc_link sections are fully handled above — skip to next
                continue;
            }

            // For parent sections: assign heading from recipe name when null
            // and sub-recipe instructions exist, so there's a visual break.
            if (hasSubRecipeInstructions && instrSection.Heading is null)
            {
                composedInstructions.Add(new Section
                {
                    Heading = recipe.Name,
                    Type = instrSection.Type,
                    BranchGroup = instrSection.BranchGroup,
                    Optional = instrSection.Optional,
                    Steps = instrSection.Steps
                });
                hasSubRecipeInstructions = false; // Only label the first null section
            }
            else
            {
                composedInstructions.Add(instrSection);
            }
        }

        return new Recipe
        {
            Name = recipe.Name,
            Version = recipe.Version,
            Author = recipe.Author,
            Description = recipe.Description,
            Status = recipe.Status,
            Ingredients = composedIngredients,
            Utensils = mergedUtensils.Count > 0 ? mergedUtensils : null,
            Instructions = composedInstructions,
            Related = recipe.Related,
            Notes = recipe.Notes,
            Yields = recipe.Yields,
            ServingSize = recipe.ServingSize,
        };
    }

    private static void MergeUtensils(List<UtensilGroup> target, List<UtensilGroup>? source)
    {
        if (source is null) return;

        var existingItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in target)
            foreach (var item in group.Items)
                existingItems.Add(item);

        foreach (var sourceGroup in source)
        {
            var newItems = new List<string>();
            foreach (var item in sourceGroup.Items)
            {
                if (existingItems.Add(item))
                    newItems.Add(item);
            }

            if (newItems.Count > 0)
            {
                // Try to merge into existing group with same heading
                var existingGroup = target.Find(g =>
                    string.Equals(g.Heading, sourceGroup.Heading, StringComparison.OrdinalIgnoreCase));

                if (existingGroup is not null)
                {
                    existingGroup.Items.AddRange(newItems);
                }
                else
                {
                    target.Add(new UtensilGroup
                    {
                        Heading = sourceGroup.Heading,
                        Items = newItems
                    });
                }
            }
        }
    }

    internal static string ResolveSubRecipePath(string? basePath, string docLink)
    {
        var linkPath = docLink.StartsWith("./", StringComparison.Ordinal) ? docLink[2..] : docLink;

        var combined = string.IsNullOrEmpty(basePath)
            ? linkPath
            : $"{basePath}/{linkPath}";

        var parts = combined.Split('/');
        var normalized = new List<string>();
        foreach (var part in parts)
        {
            if (part == "..")
            {
                if (normalized.Count > 0)
                    normalized.RemoveAt(normalized.Count - 1);
            }
            else if (part != "." && part.Length > 0)
            {
                normalized.Add(part);
            }
        }

        return string.Join("/", normalized);
    }

    private static string? GetDirectoryFromPath(string? path)
    {
        if (path is null) return null;
        var lastSlash = path.LastIndexOf('/');
        return lastSlash > 0 ? path[..lastSlash] : null;
    }
}
