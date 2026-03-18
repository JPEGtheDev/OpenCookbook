using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class RecipeComposerTests
{
    private static Recipe BuildParentRecipe(string docLink = "./Sub.yaml")
    {
        return new Recipe
        {
            Name = "Parent Recipe",
            Version = "1.0",
            Author = "Test",
            Description = "Parent",
            Status = RecipeStatus.Stable,
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 1, Unit = "whole", Name = "Sub-Recipe", DocLink = docLink },
                        new Ingredient { Quantity = 40, Unit = "g", Name = "Panko Bread Crumbs" },
                    ]
                }
            ],
            Utensils =
            [
                new UtensilGroup { Heading = null, Items = ["Mixing Bowl", "Baking Sheet"] }
            ],
            Instructions =
            [
                new Section
                {
                    Heading = null,
                    Type = SectionType.Sequence,
                    Steps = [new Step { Text = "Mix bread crumbs into the sub-recipe" }]
                }
            ]
        };
    }

    private static Recipe BuildSubRecipe()
    {
        return new Recipe
        {
            Name = "Sub-Recipe",
            Version = "1.0",
            Author = "Test",
            Description = "Child",
            Status = RecipeStatus.Stable,
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 907, Unit = "g", Name = "Ground Beef" },
                        new Ingredient { Quantity = 3, Unit = "g", Name = "Black Pepper" },
                    ]
                }
            ],
            Utensils =
            [
                new UtensilGroup { Heading = null, Items = ["Mixing Bowl", "Cheese Grater"] }
            ],
            Instructions =
            [
                new Section
                {
                    Heading = null,
                    Type = SectionType.Sequence,
                    Steps =
                    [
                        new Step { Text = "Mix all sub-recipe ingredients" },
                        new Step { Text = "Form into shape" },
                    ]
                }
            ]
        };
    }

    // ── Ingredient Merging ──────────────────────────

    [Fact]
    public async Task ComposeAsync_ReplacesDocLinkWithSubRecipeIngredients()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — doc_link item replaced with sub-recipe's ingredients
        var allItems = composed.Ingredients.SelectMany(g => g.Items).ToList();
        Assert.Contains(allItems, i => i.Name == "Ground Beef");
        Assert.Contains(allItems, i => i.Name == "Black Pepper");
        Assert.Contains(allItems, i => i.Name == "Panko Bread Crumbs");
        Assert.DoesNotContain(allItems, i => i.Name == "Sub-Recipe");
    }

    [Fact]
    public async Task ComposeAsync_SubRecipeIngredientsGetHeading()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — sub-recipe ingredients have a heading derived from the sub-recipe name
        var subGroup = composed.Ingredients.First(g => g.Items.Any(i => i.Name == "Ground Beef"));
        Assert.Equal("Sub-Recipe", subGroup.Heading);
    }

    [Fact]
    public async Task ComposeAsync_ParentIngredientsPreserved()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — parent's own ingredients still present
        var pankoGroup = composed.Ingredients.First(g => g.Items.Any(i => i.Name == "Panko Bread Crumbs"));
        Assert.Single(pankoGroup.Items);
    }

    // ── Instruction Merging ──────────────────────────

    [Fact]
    public async Task ComposeAsync_SubRecipeInstructionsPrependedBeforeParent()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — sub-recipe instructions come first
        Assert.True(composed.Instructions.Count >= 2);
        Assert.Contains("Mix all sub-recipe ingredients", composed.Instructions[0].Steps[0].Text);
        Assert.Contains("Mix bread crumbs", composed.Instructions.Last().Steps[0].Text);
    }

    [Fact]
    public async Task ComposeAsync_SubRecipeInstructionsGetHeading()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — first instruction section gets the sub-recipe name as heading
        Assert.Equal("Sub-Recipe", composed.Instructions[0].Heading);
    }

    // ── Utensil Merging ──────────────────────────

    [Fact]
    public async Task ComposeAsync_MergesUtensils_Deduplicates()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — combined utensils, "Mixing Bowl" not duplicated
        var allUtensils = composed.Utensils!.SelectMany(g => g.Items).ToList();
        Assert.Contains("Mixing Bowl", allUtensils);
        Assert.Contains("Baking Sheet", allUtensils);
        Assert.Contains("Cheese Grater", allUtensils);
        Assert.Single(allUtensils, u => u == "Mixing Bowl");
    }

    // ── Cycle Detection ──────────────────────────

    [Fact]
    public async Task ComposeAsync_DetectsCycles_StopsRecursion()
    {
        // Arrange — A → B → A (cycle)
        var recipeA = new Recipe
        {
            Name = "Recipe A",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items = [new Ingredient { Quantity = 1, Unit = "whole", Name = "Recipe B", DocLink = "./B.yaml" }]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Step A" }] }]
        };
        var recipeB = new Recipe
        {
            Name = "Recipe B",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items = [new Ingredient { Quantity = 1, Unit = "whole", Name = "Recipe A", DocLink = "./A.yaml" }]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Step B" }] }]
        };

        var repo = new FakeRecipeRepository(new()
        {
            ["dir/A.yaml"] = recipeA,
            ["dir/B.yaml"] = recipeB,
        });
        var composer = new RecipeComposer(repo);

        // Act — should not throw or infinite loop
        var composed = await composer.ComposeAsync(recipeA, "dir/A.yaml");

        // Assert — B's content merged, but B's reference back to A is skipped
        var allSteps = composed.Instructions.SelectMany(s => s.Steps).Select(s => s.Text).ToList();
        Assert.Contains("Step A", allSteps);
        Assert.Contains("Step B", allSteps);
    }

    [Fact]
    public async Task ComposeAsync_SelfReference_DoesNotInfiniteLoop()
    {
        // Arrange — recipe references itself
        var recipe = new Recipe
        {
            Name = "Self",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items = [new Ingredient { Quantity = 1, Unit = "whole", Name = "Self", DocLink = "./Self.yaml" }]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Step" }] }]
        };

        var repo = new FakeRecipeRepository(new()
        {
            ["Self.yaml"] = recipe,
        });
        var composer = new RecipeComposer(repo);

        // Act — should not throw or infinite loop
        var composed = await composer.ComposeAsync(recipe, "Self.yaml");

        // Assert — self-reference skipped, recipe has only its own instruction
        Assert.Single(composed.Instructions);
    }

    // ── Recursive Composition ──────────────────────────

    [Fact]
    public async Task ComposeAsync_RecursivelyResolvesNestedSubRecipes()
    {
        // Arrange — Grandparent → Parent → Child (3 levels)
        var child = new Recipe
        {
            Name = "Spice Blend",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 5, Unit = "g", Name = "Paprika" },
                        new Ingredient { Quantity = 3, Unit = "g", Name = "Cumin" },
                    ]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Mix spices" }] }]
        };
        var parent = new Recipe
        {
            Name = "Kebab Meat",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 907, Unit = "g", Name = "Ground Beef" },
                        new Ingredient { Quantity = 1, Unit = "batch", Name = "Spice Blend", DocLink = "./Spice_Blend.yaml" },
                    ]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Combine meat and spices" }] }]
        };
        var grandparent = new Recipe
        {
            Name = "Kebab Meatballs",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 1, Unit = "batch", Name = "Kebab Meat", DocLink = "./Kebab_Meat.yaml" },
                        new Ingredient { Quantity = 40, Unit = "g", Name = "Breadcrumbs" },
                    ]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Form meatballs" }] }]
        };

        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Kebab_Meat.yaml"] = parent,
            ["Grilling/Spice_Blend.yaml"] = child,
        });
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(grandparent, "Grilling/Kebab_Meatballs.yaml");

        // Assert — all three levels of ingredients merged
        var allItems = composed.Ingredients.SelectMany(g => g.Items).ToList();
        Assert.Contains(allItems, i => i.Name == "Ground Beef");
        Assert.Contains(allItems, i => i.Name == "Paprika");
        Assert.Contains(allItems, i => i.Name == "Cumin");
        Assert.Contains(allItems, i => i.Name == "Breadcrumbs");

        // Assert — instructions ordered: child → parent → grandparent
        var allSteps = composed.Instructions.SelectMany(s => s.Steps).Select(s => s.Text).ToList();
        var spiceIdx = allSteps.IndexOf("Mix spices");
        var combineIdx = allSteps.IndexOf("Combine meat and spices");
        var formIdx = allSteps.IndexOf("Form meatballs");
        Assert.True(spiceIdx < combineIdx, "Spice blend steps should come before kebab meat steps");
        Assert.True(combineIdx < formIdx, "Kebab meat steps should come before meatball steps");
    }

    // ── Error Handling ──────────────────────────

    [Fact]
    public async Task ComposeAsync_MissingSubRecipe_KeepsOriginalIngredient()
    {
        // Arrange — sub-recipe not found
        var repo = new FakeRecipeRepository(new());
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe("./Missing.yaml");

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — the doc_link ingredient is kept as-is
        var allItems = composed.Ingredients.SelectMany(g => g.Items).ToList();
        Assert.Contains(allItems, i => i.Name == "Sub-Recipe" && i.DocLink == "./Missing.yaml");
        Assert.Contains(allItems, i => i.Name == "Panko Bread Crumbs");
    }

    // ── No Doc Links ──────────────────────────

    [Fact]
    public async Task ComposeAsync_NoDocLinks_ReturnsEquivalentRecipe()
    {
        // Arrange — recipe with no doc_link ingredients
        var recipe = new Recipe
        {
            Name = "Simple",
            Version = "1.0",
            Author = "Test",
            Description = "No links",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Flour" },
                        new Ingredient { Quantity = 200, Unit = "ml", Name = "Milk" },
                    ]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Mix" }] }]
        };

        var repo = new FakeRecipeRepository(new());
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(recipe, "Simple.yaml");

        // Assert — recipe unchanged
        Assert.Equal(2, composed.Ingredients.SelectMany(g => g.Items).Count());
        Assert.Single(composed.Instructions);
    }

    // ── Multiple Doc Links ──────────────────────────

    [Fact]
    public async Task ComposeAsync_MultipleDocLinks_AllResolved()
    {
        // Arrange — recipe with two doc_link ingredients
        var subA = new Recipe
        {
            Name = "Sub A",
            Ingredients =
            [
                new IngredientGroup { Items = [new Ingredient { Quantity = 10, Unit = "g", Name = "Ingredient A" }] }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Step A" }] }]
        };
        var subB = new Recipe
        {
            Name = "Sub B",
            Ingredients =
            [
                new IngredientGroup { Items = [new Ingredient { Quantity = 20, Unit = "g", Name = "Ingredient B" }] }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Step B" }] }]
        };
        var parent = new Recipe
        {
            Name = "Multi-Link",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 1, Unit = "batch", Name = "Sub A", DocLink = "./SubA.yaml" },
                        new Ingredient { Quantity = 1, Unit = "batch", Name = "Sub B", DocLink = "./SubB.yaml" },
                        new Ingredient { Quantity = 50, Unit = "g", Name = "Own Ingredient" },
                    ]
                }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Combine all" }] }]
        };

        var repo = new FakeRecipeRepository(new()
        {
            ["dir/SubA.yaml"] = subA,
            ["dir/SubB.yaml"] = subB,
        });
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(parent, "dir/Parent.yaml");

        // Assert — both sub-recipes merged
        var allItems = composed.Ingredients.SelectMany(g => g.Items).ToList();
        Assert.Contains(allItems, i => i.Name == "Ingredient A");
        Assert.Contains(allItems, i => i.Name == "Ingredient B");
        Assert.Contains(allItems, i => i.Name == "Own Ingredient");

        // Assert — both sub-recipe instructions prepended
        var allSteps = composed.Instructions.SelectMany(s => s.Steps).Select(s => s.Text).ToList();
        var stepA = allSteps.IndexOf("Step A");
        var stepB = allSteps.IndexOf("Step B");
        var combine = allSteps.IndexOf("Combine all");
        Assert.True(stepA < combine);
        Assert.True(stepB < combine);
    }

    // ── Path Resolution ──────────────────────────

    [Theory]
    [InlineData("Grilling", "./Sub.yaml", "Grilling/Sub.yaml")]
    [InlineData("Grilling", "Sub.yaml", "Grilling/Sub.yaml")]
    [InlineData(null, "./Sub.yaml", "Sub.yaml")]
    [InlineData("a/b", "../c/Sub.yaml", "a/c/Sub.yaml")]
    public void ResolveSubRecipePath_ReturnsExpectedPath(string? basePath, string docLink, string expected)
    {
        var result = RecipeComposer.ResolveSubRecipePath(basePath, docLink);
        Assert.Equal(expected, result);
    }

    // ── Original Recipe Not Mutated ──────────────────────────

    [Fact]
    public async Task ComposeAsync_DoesNotMutateOriginalRecipe()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();
        var originalIngredientCount = parent.Ingredients.SelectMany(g => g.Items).Count();
        var originalInstructionCount = parent.Instructions.Count;

        // Act
        _ = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — original recipe is untouched
        Assert.Equal(originalIngredientCount, parent.Ingredients.SelectMany(g => g.Items).Count());
        Assert.Equal(originalInstructionCount, parent.Instructions.Count);
    }

    // ── Parent Instruction Heading ──────────────────────────

    [Fact]
    public async Task ComposeAsync_ParentNullHeadingSection_GetsRecipeName()
    {
        // Arrange — parent has heading: null on its first instruction section
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — parent's null-headed section now has the recipe name as heading
        var parentSection = composed.Instructions.Last();
        Assert.Equal("Parent Recipe", parentSection.Heading);
    }

    [Fact]
    public async Task ComposeAsync_ParentWithExistingHeading_NotOverwritten()
    {
        // Arrange — parent has an explicit heading on its instruction section
        var repo = new FakeRecipeRepository(new()
        {
            ["Grilling/Sub.yaml"] = BuildSubRecipe()
        });
        var composer = new RecipeComposer(repo);
        var parent = BuildParentRecipe();
        parent.Instructions[0].Heading = "Custom Heading";

        // Act
        var composed = await composer.ComposeAsync(parent, "Grilling/Parent.yaml");

        // Assert — the explicit heading is preserved
        var parentSection = composed.Instructions.Last();
        Assert.Equal("Custom Heading", parentSection.Heading);
    }

    [Fact]
    public async Task ComposeAsync_NoSubRecipes_NullHeadingPreserved()
    {
        // Arrange — recipe with no doc_links
        var recipe = new Recipe
        {
            Name = "Simple",
            Ingredients =
            [
                new IngredientGroup { Items = [new Ingredient { Quantity = 100, Unit = "g", Name = "Flour" }] }
            ],
            Instructions = [new Section { Heading = null, Steps = [new Step { Text = "Mix" }] }]
        };
        var repo = new FakeRecipeRepository(new());
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(recipe, "Simple.yaml");

        // Assert — no sub-recipes means null heading stays null
        Assert.Null(composed.Instructions[0].Heading);
    }

    // ── Instruction-Level DocLink ──────────────────────────

    [Fact]
    public async Task ComposeAsync_InstructionDocLink_InsertsAtPosition()
    {
        // Arrange — recipe with doc_link on an instruction section (mid-flow)
        var herbButter = new Recipe
        {
            Name = "Herb Butter",
            Ingredients =
            [
                new IngredientGroup { Items = [new Ingredient { Quantity = 200, Unit = "g", Name = "Butter" }] }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Mix butter and herbs" }] }]
        };
        var parent = new Recipe
        {
            Name = "Kiev Cutlet",
            Ingredients =
            [
                new IngredientGroup { Items = [new Ingredient { Quantity = 500, Unit = "g", Name = "Chicken" }] }
            ],
            Instructions =
            [
                new Section { Heading = "Chicken Prep", Steps = [new Step { Text = "Season chicken" }] },
                new Section { DocLink = "./Herb_Butter.yaml" },
                new Section { Heading = "Forming", Steps = [new Step { Text = "Form cutlets" }] },
            ]
        };

        var repo = new FakeRecipeRepository(new()
        {
            ["dir/Herb_Butter.yaml"] = herbButter
        });
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(parent, "dir/Kiev.yaml");

        // Assert — instructions are in the right order
        var allSteps = composed.Instructions.SelectMany(s => s.Steps).Select(s => s.Text).ToList();
        var seasonIdx = allSteps.IndexOf("Season chicken");
        var butterIdx = allSteps.IndexOf("Mix butter and herbs");
        var formIdx = allSteps.IndexOf("Form cutlets");
        Assert.True(seasonIdx < butterIdx, "Chicken prep should come before herb butter");
        Assert.True(butterIdx < formIdx, "Herb butter should come before forming");
    }

    [Fact]
    public async Task ComposeAsync_InstructionDocLink_MergesIngredients()
    {
        // Arrange
        var herbButter = new Recipe
        {
            Name = "Herb Butter",
            Ingredients =
            [
                new IngredientGroup { Items = [new Ingredient { Quantity = 200, Unit = "g", Name = "Butter" }] }
            ],
            Instructions = [new Section { Steps = [new Step { Text = "Mix butter" }] }]
        };
        var parent = new Recipe
        {
            Name = "Test",
            Ingredients =
            [
                new IngredientGroup { Items = [new Ingredient { Quantity = 500, Unit = "g", Name = "Chicken" }] }
            ],
            Instructions =
            [
                new Section { Heading = "Prep", Steps = [new Step { Text = "Prep" }] },
                new Section { DocLink = "./HB.yaml" },
            ]
        };

        var repo = new FakeRecipeRepository(new()
        {
            ["dir/HB.yaml"] = herbButter
        });
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(parent, "dir/Test.yaml");

        // Assert — sub-recipe ingredients from instruction doc_link are merged
        var allItems = composed.Ingredients.SelectMany(g => g.Items).ToList();
        Assert.Contains(allItems, i => i.Name == "Chicken");
        Assert.Contains(allItems, i => i.Name == "Butter");
    }

    [Fact]
    public async Task ComposeAsync_InstructionDocLink_GetsHeading()
    {
        // Arrange
        var sub = new Recipe
        {
            Name = "Herb Butter",
            Ingredients = [new IngredientGroup { Items = [] }],
            Instructions = [new Section { Heading = null, Steps = [new Step { Text = "Mix" }] }]
        };
        var parent = new Recipe
        {
            Name = "Parent",
            Ingredients = [new IngredientGroup { Items = [] }],
            Instructions = [new Section { DocLink = "./Sub.yaml" }]
        };

        var repo = new FakeRecipeRepository(new() { ["dir/Sub.yaml"] = sub });
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(parent, "dir/Parent.yaml");

        // Assert — inserted instruction section gets the sub-recipe name as heading
        Assert.Equal("Herb Butter", composed.Instructions[0].Heading);
    }

    [Fact]
    public async Task ComposeAsync_InstructionDocLink_MissingRecipe_SectionSkipped()
    {
        // Arrange — doc_link points to non-existent recipe
        var parent = new Recipe
        {
            Name = "Parent",
            Ingredients = [new IngredientGroup { Items = [] }],
            Instructions =
            [
                new Section { Heading = "Step 1", Steps = [new Step { Text = "Do stuff" }] },
                new Section { DocLink = "./Missing.yaml" },
                new Section { Heading = "Step 3", Steps = [new Step { Text = "Finish" }] },
            ]
        };

        var repo = new FakeRecipeRepository(new());
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(parent, "dir/Parent.yaml");

        // Assert — missing doc_link section is skipped, other sections preserved
        Assert.Equal(2, composed.Instructions.Count);
        Assert.Equal("Step 1", composed.Instructions[0].Heading);
        Assert.Equal("Step 3", composed.Instructions[1].Heading);
    }

    [Fact]
    public async Task ComposeAsync_InstructionDocLink_CycleDetected_SectionSkipped()
    {
        // Arrange — instruction doc_link points to already-visited recipe
        var parent = new Recipe
        {
            Name = "Parent",
            Ingredients = [new IngredientGroup { Items = [] }],
            Instructions =
            [
                new Section { Steps = [new Step { Text = "Parent step" }] },
                new Section { DocLink = "./Parent.yaml" }, // Self-reference via instruction
            ]
        };

        var repo = new FakeRecipeRepository(new() { ["dir/Parent.yaml"] = parent });
        var composer = new RecipeComposer(repo);

        // Act
        var composed = await composer.ComposeAsync(parent, "dir/Parent.yaml");

        // Assert — self-referencing instruction doc_link is skipped
        Assert.Single(composed.Instructions);
    }
}
