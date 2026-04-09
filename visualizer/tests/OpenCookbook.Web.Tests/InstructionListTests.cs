using Bunit;
using OpenCookbook.Domain.Entities;
using OpenCookbook.Web.Components;

namespace OpenCookbook.Web.Tests;

public class InstructionListTests : BunitContext
{
    [Fact]
    public void InstructionList_SuppressStorageFalse_RendersStorageSection()
    {
        // Arrange
        var sections = new List<Section>
        {
            new()
            {
                Heading = "Prep",
                Steps = [new Step { Text = "Mix ingredients" }]
            },
            new()
            {
                Heading = "Storage",
                Type = SectionType.Storage,
                Optional = true,
                Steps = [new Step { Text = "Freeze for 3 months" }]
            }
        };

        // Act
        var cut = Render<InstructionList>(p => p
            .Add(x => x.Sections, sections)
            .Add(x => x.SuppressStorage, false));

        // Assert — both sections rendered
        var headings = cut.FindAll(".section-heading");
        Assert.Equal(2, headings.Count);
        Assert.Contains("Prep", headings[0].TextContent);
        Assert.Contains("Storage", headings[1].TextContent);
    }

    [Fact]
    public void InstructionList_SuppressStorageTrue_HidesStorageSection()
    {
        // Arrange
        var sections = new List<Section>
        {
            new()
            {
                Heading = "Prep",
                Steps = [new Step { Text = "Mix ingredients" }]
            },
            new()
            {
                Heading = "Storage",
                Type = SectionType.Storage,
                Optional = true,
                Steps = [new Step { Text = "Freeze for 3 months" }]
            }
        };

        // Act
        var cut = Render<InstructionList>(p => p
            .Add(x => x.Sections, sections)
            .Add(x => x.SuppressStorage, true));

        // Assert — only the Prep section is rendered
        var headings = cut.FindAll(".section-heading");
        Assert.Single(headings);
        Assert.Contains("Prep", headings[0].TextContent);
    }

    [Fact]
    public void InstructionList_SuppressStorageTrue_NonStorageSectionsRendered()
    {
        // Arrange
        var sections = new List<Section>
        {
            new()
            {
                Heading = "Marinating",
                Steps = [new Step { Text = "Combine spices" }]
            },
            new()
            {
                Heading = "Cooking",
                Steps = [new Step { Text = "Cook over medium heat" }]
            }
        };

        // Act
        var cut = Render<InstructionList>(p => p
            .Add(x => x.Sections, sections)
            .Add(x => x.SuppressStorage, true));

        // Assert — all non-storage sections still render
        var headings = cut.FindAll(".section-heading");
        Assert.Equal(2, headings.Count);
    }

    [Fact]
    public void InstructionList_OptionalStorageSection_DoesNotGetInstructionOptionalClass()
    {
        // Arrange — optional storage section should NOT get the indented styling
        var sections = new List<Section>
        {
            new()
            {
                Heading = "Prep",
                Optional = true,
                Steps = [new Step { Text = "Mix ingredients" }]
            },
            new()
            {
                Heading = "Freezing",
                Type = SectionType.Storage,
                Optional = true,
                Steps = [new Step { Text = "Freeze for 3 months" }]
            }
        };

        // Act
        var cut = Render<InstructionList>(p => p
            .Add(x => x.Sections, sections));

        // Assert — Prep (optional, non-storage) gets instruction-optional; Freezing (optional, storage) does not
        var instructionSections = cut.FindAll(".instruction-section");
        Assert.Equal(2, instructionSections.Count);
        Assert.Contains("instruction-optional", instructionSections[0].ClassList);
        Assert.DoesNotContain("instruction-optional", instructionSections[1].ClassList);
    }

    [Fact]
    public void InstructionList_DefaultSuppressStorage_IsFalse()
    {
        // Arrange — section with storage should render by default
        var sections = new List<Section>
        {
            new()
            {
                Heading = "Storage",
                Type = SectionType.Storage,
                Steps = [new Step { Text = "Freeze" }]
            }
        };

        // Act — do not set SuppressStorage (should default to false)
        var cut = Render<InstructionList>(p => p
            .Add(x => x.Sections, sections));

        // Assert — storage section is rendered by default
        var headings = cut.FindAll(".section-heading");
        Assert.Single(headings);
        Assert.Contains("Storage", headings[0].TextContent);
    }
}
