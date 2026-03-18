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

        return await ComposeRecursiveAsync(recipe, DocLinkResolver.GetDirectory(recipePath), visited);
    }

    private async Task<Recipe> ComposeRecursiveAsync(
        Recipe recipe,
        string? basePath,
        HashSet<string> visited)
    {
        var composedIngredients = new List<IngredientGroup>();
        var prependedInstructions = new List<Section>();

        // Deep-clone parent utensils so MergeUtensils never mutates the original recipe
        var mergedUtensils = DeepCloneUtensils(recipe.Utensils);

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

                var resolvedPath = DocLinkResolver.ResolvePath(basePath, item.DocLink);

                // Cycle detection: skip if currently on the recursion stack
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
                var subBasePath = DocLinkResolver.GetDirectory(resolvedPath);
                subRecipe = await ComposeRecursiveAsync(subRecipe, subBasePath, visited);

                // Remove from recursion stack after processing so the same sub-recipe
                // can be referenced from a different parent (DAG reuse).
                visited.Remove(resolvedPath);

                // Merge sub-recipe ingredients under a heading, scaled by the
                // referencing ingredient's quantity (e.g. Quantity=2 → 2× amounts).
                var scale = item.Quantity;
                foreach (var subGroup in subRecipe.Ingredients)
                {
                    composedIngredients.Add(new IngredientGroup
                    {
                        Heading = subGroup.Heading ?? subRecipe.Name,
                        Items = ScaleIngredients(subGroup.Items, scale)
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
                var resolvedPath = DocLinkResolver.ResolvePath(basePath, instrSection.DocLink);

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
                        var subBasePath = DocLinkResolver.GetDirectory(resolvedPath);
                        linkedRecipe = await ComposeRecursiveAsync(linkedRecipe, subBasePath, visited);

                        // Remove from recursion stack after processing (DAG reuse)
                        visited.Remove(resolvedPath);

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
                    else
                    {
                        // Recipe failed to load from repository — remove from visited
                        // so it doesn't block future references to the same path.
                        visited.Remove(resolvedPath);
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
                hasSubRecipeInstructions = false; // Reset: only the first null-headed section after sub-recipe instructions gets labeled
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

    /// <summary>
    /// Returns new <see cref="Ingredient"/> instances with quantities scaled by the
    /// given <paramref name="scale"/> factor. When scale is 1 the original list is
    /// returned without allocation.
    /// </summary>
    private static List<Ingredient> ScaleIngredients(List<Ingredient> items, double scale)
    {
        // Exact 1.0 check is safe here — Quantity values come from YAML deserialization
        // and are assigned as literal doubles, not computed via floating-point arithmetic.
        if (scale == 1.0)
            return items;

        return items.Select(i => new Ingredient
        {
            Quantity = i.Quantity * scale,
            Unit = i.Unit,
            Name = i.Name,
            VolumeAlt = i.VolumeAlt,
            Note = i.Note,
            NutritionId = i.NutritionId,
            DocLink = i.DocLink,
        }).ToList();
    }

    /// <summary>
    /// Deep-clones the utensil groups so that <see cref="MergeUtensils"/> never
    /// mutates the original recipe's data.
    /// </summary>
    private static List<UtensilGroup> DeepCloneUtensils(List<UtensilGroup>? source)
    {
        if (source is null || source.Count == 0)
            return [];

        return source.Select(g => new UtensilGroup
        {
            Heading = g.Heading,
            Items = new List<string>(g.Items)
        }).ToList();
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

    /// <summary>
    /// Kept for backward compatibility with existing tests that call this method directly.
    /// Delegates to <see cref="DocLinkResolver.ResolvePath"/>.
    /// </summary>
    internal static string ResolveSubRecipePath(string? basePath, string docLink)
        => DocLinkResolver.ResolvePath(basePath, docLink);
}
