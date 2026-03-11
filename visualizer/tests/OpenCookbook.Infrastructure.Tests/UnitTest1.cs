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
        var recipe = _parser.Parse(SimpleRecipeYaml);
        Assert.Equal("Guajillo Brisket Rub", recipe.Name);
    }

    [Fact]
    public void Parse_SimpleRecipe_ReturnsCorrectVersion()
    {
        var recipe = _parser.Parse(SimpleRecipeYaml);
        Assert.Equal("1.0", recipe.Version);
    }

    [Fact]
    public void Parse_SimpleRecipe_ReturnsCorrectAuthor()
    {
        var recipe = _parser.Parse(SimpleRecipeYaml);
        Assert.Equal("Jonathan Petz | JPEGtheDev", recipe.Author);
    }

    [Fact]
    public void Parse_SimpleRecipe_ReturnsStableStatus()
    {
        var recipe = _parser.Parse(SimpleRecipeYaml);
        Assert.Equal(RecipeStatus.Stable, recipe.Status);
    }

    [Fact]
    public void Parse_SimpleRecipe_ParsesIngredients()
    {
        var recipe = _parser.Parse(SimpleRecipeYaml);

        Assert.Single(recipe.Ingredients);
        var group = recipe.Ingredients[0];
        Assert.Null(group.Heading);
        Assert.Equal(2, group.Items.Count);
        Assert.Equal("Guajillo Chiles", group.Items[0].Name);
        Assert.Equal(100, group.Items[0].Quantity);
        Assert.Equal("g", group.Items[0].Unit);
    }

    [Fact]
    public void Parse_SimpleRecipe_ParsesInstructions()
    {
        var recipe = _parser.Parse(SimpleRecipeYaml);

        Assert.Single(recipe.Instructions);
        var section = recipe.Instructions[0];
        Assert.Equal(SectionType.Sequence, section.Type);
        Assert.Equal(2, section.Steps.Count);
    }

    [Fact]
    public void Parse_SimpleRecipe_ParsesStepNotes()
    {
        var recipe = _parser.Parse(SimpleRecipeYaml);

        var step = recipe.Instructions[0].Steps[1];
        Assert.NotNull(step.Notes);
        Assert.Single(step.Notes);
        Assert.Contains("mustard seeds", step.Notes[0]);
    }

    [Fact]
    public void Parse_SimpleRecipe_ParsesRelated()
    {
        var recipe = _parser.Parse(SimpleRecipeYaml);

        Assert.NotNull(recipe.Related);
        Assert.Single(recipe.Related);
        Assert.Equal("Guajillo Brisket Binder", recipe.Related[0].Label);
        Assert.Equal("./Guajillo_Brisket_Binder.yaml", recipe.Related[0].Path);
    }

    [Fact]
    public void Parse_BranchRecipe_ParsesBranchSections()
    {
        var recipe = _parser.Parse(BranchRecipeYaml);

        var branches = recipe.Instructions
            .Where(s => s.Type == SectionType.Branch)
            .ToList();

        Assert.Equal(2, branches.Count);
        Assert.Equal("Grilled", branches[0].Heading);
        Assert.Equal("Baked", branches[1].Heading);
        Assert.Equal("cooking-method", branches[0].BranchGroup);
        Assert.Equal("cooking-method", branches[1].BranchGroup);
    }

    [Fact]
    public void Parse_BranchRecipe_ParsesVolumeAlt()
    {
        var recipe = _parser.Parse(BranchRecipeYaml);

        var pepper = recipe.Ingredients[0].Items[1];
        Assert.Equal("Black Pepper", pepper.Name);
        Assert.Equal("3/4 tsp.", pepper.VolumeAlt);
    }

    [Fact]
    public void Parse_BranchRecipe_ParsesOptionalSection()
    {
        var recipe = _parser.Parse(BranchRecipeYaml);

        var freezing = recipe.Instructions.First(s => s.Heading == "Freezing");
        Assert.True(freezing.Optional);
    }

    [Fact]
    public void Parse_BranchRecipe_MaintainsSectionOrder()
    {
        var recipe = _parser.Parse(BranchRecipeYaml);

        Assert.Equal(5, recipe.Instructions.Count);
        Assert.Equal("Marinating", recipe.Instructions[0].Heading);
        Assert.Equal("Grilled", recipe.Instructions[1].Heading);
        Assert.Equal("Baked", recipe.Instructions[2].Heading);
        Assert.Equal("Serving", recipe.Instructions[3].Heading);
        Assert.Equal("Freezing", recipe.Instructions[4].Heading);
    }

    [Theory]
    [InlineData("status: stable", RecipeStatus.Stable)]
    [InlineData("status: beta", RecipeStatus.Beta)]
    [InlineData("status: draft", RecipeStatus.Draft)]
    public void Parse_StatusValues_AreHandledCorrectly(string statusLine, RecipeStatus expected)
    {
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

        var recipe = _parser.Parse(yaml);
        Assert.Equal(expected, recipe.Status);
    }

    [Fact]
    public void Parse_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _parser.Parse(""));
    }

    [Fact]
    public void Parse_WhitespaceOnly_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _parser.Parse("   "));
    }
}
