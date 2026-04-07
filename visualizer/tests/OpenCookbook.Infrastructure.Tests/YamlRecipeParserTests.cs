using OpenCookbook.Domain.Entities;
using OpenCookbook.Infrastructure.Parsing;

namespace OpenCookbook.Infrastructure.Tests;

public class YamlRecipeParserTests
{
    private readonly YamlRecipeParser _parser = new();

    private const string SimpleRecipeYaml = """
        name: Guajillo Brisket Rub
        version: "1.0"
        author: Jonathan Petz | JPEGtheDev
        description: >
          A proven brisket rub that is a bit spicy and sweet.
        status: stable

        ingredients:
          - heading: null
            items:
              - quantity: 100
                unit: g
                name: Guajillo Chiles
              - quantity: 300
                unit: g
                name: Fine Sea Salt

        instructions:
          - heading: null
            type: sequence
            steps:
              - text: Remove the stems from the chiles by hand
              - text: Grind chiles in a food processor
                notes:
                  - Grind mustard seeds at this step too.

        related:
          - label: Guajillo Brisket Binder
            path: ./Guajillo_Brisket_Binder.yaml
        """;

    private const string BranchRecipeYaml = """
        name: Chicken Shawarma
        version: "1.1"
        author: Jonathan Petz | JPEGtheDev
        description: Chicken shawarma with grilled or baked option.
        status: stable

        ingredients:
          - heading: null
            items:
              - quantity: 1300
                unit: g
                name: Chicken Thighs
              - quantity: 1.7
                unit: g
                name: Black Pepper
                volume_alt: "3/4 tsp."

        instructions:
          - heading: Marinating
            type: sequence
            steps:
              - text: Mix spices together
          - heading: Grilled
            type: branch
            branch_group: cooking-method
            steps:
              - text: Preheat grill to 218°C (425°F)
          - heading: Baked
            type: branch
            branch_group: cooking-method
            steps:
              - text: Preheat oven to 218°C (425°F)
          - heading: Serving
            type: sequence
            steps:
              - text: Let chicken rest for 8 minutes
          - heading: Freezing
            type: sequence
            optional: true
            steps:
              - text: Flash freeze individual cubes
        """;

    [Fact]
    public void Parse_SimpleRecipe_ReturnsCorrectName()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Guajillo Brisket Rub", recipe.Name);
    }

    [Fact]
    public void Parse_SimpleRecipe_ReturnsCorrectVersion()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("1.0", recipe.Version);
    }

    [Fact]
    public void Parse_SimpleRecipe_ReturnsCorrectAuthor()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Jonathan Petz | JPEGtheDev", recipe.Author);
    }

    [Fact]
    public void Parse_SimpleRecipe_ReturnsStableStatus()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(RecipeStatus.Stable, recipe.Status);
    }

    [Fact]
    public void Parse_SimpleRecipe_HasOneIngredientGroup()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Single(recipe.Ingredients);
    }

    [Fact]
    public void Parse_SimpleRecipe_IngredientGroupHeadingIsNull()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Null(recipe.Ingredients[0].Heading);
    }

    [Fact]
    public void Parse_SimpleRecipe_IngredientGroupHasTwoItems()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(2, recipe.Ingredients[0].Items.Count);
    }

    [Fact]
    public void Parse_SimpleRecipe_FirstIngredientNameIsCorrect()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Guajillo Chiles", recipe.Ingredients[0].Items[0].Name);
    }

    [Fact]
    public void Parse_SimpleRecipe_FirstIngredientQuantityIsCorrect()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(100, recipe.Ingredients[0].Items[0].Quantity);
    }

    [Fact]
    public void Parse_SimpleRecipe_FirstIngredientUnitIsCorrect()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("g", recipe.Ingredients[0].Items[0].Unit);
    }

    [Fact]
    public void Parse_SimpleRecipe_HasOneInstructionSection()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Single(recipe.Instructions);
    }

    [Fact]
    public void Parse_SimpleRecipe_InstructionTypeIsSequence()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(SectionType.Sequence, recipe.Instructions[0].Type);
    }

    [Fact]
    public void Parse_SimpleRecipe_InstructionHasTwoSteps()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(2, recipe.Instructions[0].Steps.Count);
    }

    [Fact]
    public void Parse_SimpleRecipe_SecondStepHasNotes()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.NotNull(recipe.Instructions[0].Steps[1].Notes);
    }

    [Fact]
    public void Parse_SimpleRecipe_SecondStepHasOneNote()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Single(recipe.Instructions[0].Steps[1].Notes!);
    }

    [Fact]
    public void Parse_SimpleRecipe_SecondStepNoteContainsMustardSeeds()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Contains("mustard seeds", recipe.Instructions[0].Steps[1].Notes![0]);
    }

    [Fact]
    public void Parse_SimpleRecipe_HasRelatedRecipes()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.NotNull(recipe.Related);
    }

    [Fact]
    public void Parse_SimpleRecipe_HasOneRelatedRecipe()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Single(recipe.Related!);
    }

    [Fact]
    public void Parse_SimpleRecipe_RelatedLabelIsCorrect()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Guajillo Brisket Binder", recipe.Related![0].Label);
    }

    [Fact]
    public void Parse_SimpleRecipe_RelatedPathIsCorrect()
    {
        // Arrange
        var yaml = SimpleRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("./Guajillo_Brisket_Binder.yaml", recipe.Related![0].Path);
    }

    [Fact]
    public void Parse_BranchRecipe_HasTwoBranchSections()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        var branchCount = recipe.Instructions.Count(s => s.Type == SectionType.Branch);
        Assert.Equal(2, branchCount);
    }

    [Fact]
    public void Parse_BranchRecipe_FirstBranchHeadingIsGrilled()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        var firstBranch = recipe.Instructions.First(s => s.Type == SectionType.Branch);
        Assert.Equal("Grilled", firstBranch.Heading);
    }

    [Fact]
    public void Parse_BranchRecipe_SecondBranchHeadingIsBaked()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        var secondBranch = recipe.Instructions.Where(s => s.Type == SectionType.Branch).Skip(1).First();
        Assert.Equal("Baked", secondBranch.Heading);
    }

    [Fact]
    public void Parse_BranchRecipe_FirstBranchGroupIsCookingMethod()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        var firstBranch = recipe.Instructions.First(s => s.Type == SectionType.Branch);
        Assert.Equal("cooking-method", firstBranch.BranchGroup);
    }

    [Fact]
    public void Parse_BranchRecipe_SecondBranchGroupIsCookingMethod()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        var secondBranch = recipe.Instructions.Where(s => s.Type == SectionType.Branch).Skip(1).First();
        Assert.Equal("cooking-method", secondBranch.BranchGroup);
    }

    [Fact]
    public void Parse_BranchRecipe_VolumeAltIngredientNameIsCorrect()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Black Pepper", recipe.Ingredients[0].Items[1].Name);
    }

    [Fact]
    public void Parse_BranchRecipe_VolumeAltValueIsCorrect()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("3/4 tsp.", recipe.Ingredients[0].Items[1].VolumeAlt);
    }

    [Fact]
    public void Parse_BranchRecipe_FreezingSectionIsOptional()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        var freezing = recipe.Instructions.First(s => s.Heading == "Freezing");
        Assert.True(freezing.Optional);
    }

    [Fact]
    public void Parse_BranchRecipe_HasFiveSections()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(5, recipe.Instructions.Count);
    }

    [Fact]
    public void Parse_BranchRecipe_FirstSectionIsMarinating()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Marinating", recipe.Instructions[0].Heading);
    }

    [Fact]
    public void Parse_BranchRecipe_SecondSectionIsGrilled()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Grilled", recipe.Instructions[1].Heading);
    }

    [Fact]
    public void Parse_BranchRecipe_ThirdSectionIsBaked()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Baked", recipe.Instructions[2].Heading);
    }

    [Fact]
    public void Parse_BranchRecipe_FourthSectionIsServing()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Serving", recipe.Instructions[3].Heading);
    }

    [Fact]
    public void Parse_BranchRecipe_FifthSectionIsFreezing()
    {
        // Arrange
        var yaml = BranchRecipeYaml;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Freezing", recipe.Instructions[4].Heading);
    }

    [Theory]
    [InlineData("status: stable", RecipeStatus.Stable)]
    [InlineData("status: beta", RecipeStatus.Beta)]
    [InlineData("status: draft", RecipeStatus.Draft)]
    public void Parse_StatusValues_AreHandledCorrectly(string statusLine, RecipeStatus expected)
    {
        // Arrange
        var yaml = $"""
            name: Test
            version: "1.0"
            author: Test
            description: Test
            {statusLine}
            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Test
            instructions: []
            """;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(expected, recipe.Status);
    }

    [Fact]
    public void Parse_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var yaml = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _parser.Parse(yaml));
    }

    [Fact]
    public void Parse_WhitespaceOnly_ThrowsArgumentException()
    {
        // Arrange
        var yaml = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _parser.Parse(yaml));
    }

    [Fact]
    public void Parse_IngredientWithNutritionId_DeserializesToExpectedGuid()
    {
        // Arrange
        var yaml = """
            name: Test Recipe
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Chicken Wings
                    nutrition_id: "91538d7b-584e-5ba7-a374-0ce39c67dbc4"

            instructions: []
            """;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Equal(
            new Guid("91538d7b-584e-5ba7-a374-0ce39c67dbc4"),
            recipe.Ingredients[0].Items[0].NutritionId);
    }

    [Fact]
    public void Parse_IngredientWithoutNutritionId_HasNullNutritionId()
    {
        // Arrange
        var yaml = """
            name: Test Recipe
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Guajillo Chiles

            instructions: []
            """;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Null(recipe.Ingredients[0].Items[0].NutritionId);
    }

    [Fact]
    public void Parse_WithYields_ParsesQuantityAndUnit()
    {
        // Arrange
        var yaml = """
            name: Kebab Meatballs
            version: "1.0"
            author: Test
            description: Test
            status: stable

            yields:
              quantity: 24
              unit: meatball

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions: []
            """;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.NotNull(recipe.Yields);
        Assert.Equal(24, recipe.Yields!.Quantity);
        Assert.Equal("meatball", recipe.Yields.Unit);
    }

    [Fact]
    public void Parse_WithServingSize_ParsesQuantityAndUnit()
    {
        // Arrange
        var yaml = """
            name: Kebab Meatballs
            version: "1.0"
            author: Test
            description: Test
            status: stable

            yields:
              quantity: 24
              unit: meatball

            serving_size:
              quantity: 3
              unit: meatball

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions: []
            """;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.NotNull(recipe.ServingSize);
        Assert.Equal(3, recipe.ServingSize!.Quantity);
        Assert.Equal("meatball", recipe.ServingSize.Unit);
    }

    [Fact]
    public void Parse_WithoutYields_YieldsIsNull()
    {
        // Arrange
        var yaml = """
            name: Test Recipe
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions: []
            """;

        // Act
        var recipe = _parser.Parse(yaml);

        // Assert
        Assert.Null(recipe.Yields);
        Assert.Null(recipe.ServingSize);
    }

    // ── Alternates Parsing ──────────────────────────

    [Fact]
    public void Parse_IngredientWithAlternates_ParsesAlternatesList()
    {
        var yaml = """
            name: Test
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 907
                    unit: g
                    name: 88/12 Ground Beef
                    nutrition_id: "2724c62f-1832-5ccf-97b0-d219812368d8"
                    alternates:
                      - name: 80/20 Ground Beef
                        nutrition_id: "ca7b2dfc-90a9-57e5-995d-605dfef2baf8"
                        note: "For a balanced result"
                      - name: 70/30 Ground Beef
                        quantity: 900
                        unit: g

            instructions: []
            """;

        var recipe = _parser.Parse(yaml);

        var beef = recipe.Ingredients[0].Items[0];
        Assert.Equal("88/12 Ground Beef", beef.Name);
        Assert.NotNull(beef.Alternates);
        Assert.Equal(2, beef.Alternates!.Count);

        Assert.Equal("80/20 Ground Beef", beef.Alternates[0].Name);
        Assert.Equal(Guid.Parse("ca7b2dfc-90a9-57e5-995d-605dfef2baf8"), beef.Alternates[0].NutritionId);
        Assert.Equal("For a balanced result", beef.Alternates[0].Note);
        Assert.Null(beef.Alternates[0].Quantity);

        Assert.Equal("70/30 Ground Beef", beef.Alternates[1].Name);
        Assert.Equal(900, beef.Alternates[1].Quantity);
        Assert.Equal("g", beef.Alternates[1].Unit);
    }

    [Fact]
    public void Parse_IngredientWithoutAlternates_AlternatesIsNull()
    {
        var yaml = """
            name: Test
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions: []
            """;

        var recipe = _parser.Parse(yaml);

        Assert.Null(recipe.Ingredients[0].Items[0].Alternates);
    }

    // ── Storage Type Parsing ──────────────────────────

    [Fact]
    public void Parse_SectionWithStorageType_ParsesCorrectly()
    {
        var yaml = """
            name: Test
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions:
              - heading: Storage
                type: storage
                optional: true
                steps:
                  - text: Allow to cool and freeze
            """;

        var recipe = _parser.Parse(yaml);

        Assert.Single(recipe.Instructions);
        Assert.Equal(SectionType.Storage, recipe.Instructions[0].Type);
    }

    [Fact]
    public void Parse_SectionWithoutType_DefaultsToSequence()
    {
        var yaml = """
            name: Test
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions:
              - heading: null
                steps:
                  - text: Step one
            """;

        var recipe = _parser.Parse(yaml);

        Assert.Equal(SectionType.Sequence, recipe.Instructions[0].Type);
    }

    [Fact]
    public void Parse_StorageTypeCaseInsensitive_ParsesCorrectly()
    {
        var yaml = """
            name: Test
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions:
              - heading: Storage
                type: Storage
                steps:
                  - text: Freeze for up to 3 months
            """;

        var recipe = _parser.Parse(yaml);

        Assert.Equal(SectionType.Storage, recipe.Instructions[0].Type);
    }

    [Fact]
    public void Parse_InvalidType_ThrowsInvalidOperationException()
    {
        var yaml = """
            name: Test
            version: "1.0"
            author: Test
            description: Test
            status: stable

            ingredients:
              - heading: null
                items:
                  - quantity: 100
                    unit: g
                    name: Ground Beef

            instructions:
              - heading: null
                type: invalid
                steps:
                  - text: Step
            """;

        Assert.Throws<InvalidOperationException>(() => _parser.Parse(yaml));
    }

    [Fact]
    public void Parse_BranchRecipe_FreezingSectionIsSequenceType()
    {
        var yaml = BranchRecipeYaml;

        var recipe = _parser.Parse(yaml);

        var freezing = recipe.Instructions.First(s => s.Heading == "Freezing");
        Assert.Equal(SectionType.Sequence, freezing.Type);
    }
}
