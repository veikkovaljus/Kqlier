using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class EdgeCaseTests : KqlierTestBase
{
    private static bool IsUnix => OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();

    [Fact]
    public async Task EdgeCase_EmptyFile_HandledGracefully()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.EmptyKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert - should not crash
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task EdgeCase_WhitespaceOnlyFile_HandledGracefully()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.WhitespaceOnlyKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert - should not crash
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task EdgeCase_DeepNesting_DiscoveredCorrectly()
    {
        // Arrange - create files 5 levels deep
        TempDir.CreateFile("a/b/c/d/e/deep.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("a/b/c/d/e/deep.kql"));
    }

    [Fact]
    public async Task EdgeCase_SpecialCharactersInPath_HandledCorrectly()
    {
        // Arrange - directory with spaces
        TempDir.CreateFile("path with spaces/test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("path with spaces/test.kql"));
    }

    [Fact]
    public async Task EdgeCase_ManyFiles_AllProcessed()
    {
        // Arrange - create 20 files
        for (int i = 0; i < 20; i++)
        {
            TempDir.CreateFile($"file{i}.kql", TestFileFactory.UnformattedKql);
        }

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Formatted 20 of 20 files", result.StandardOutput);

        // Verify all files are formatted
        for (int i = 0; i < 20; i++)
        {
            Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile($"file{i}.kql"));
        }
    }

    [Fact]
    public async Task EdgeCase_HiddenDirectory_Skipped()
    {
        // A leading dot only maps to the hidden attribute on Unix.
        Assert.SkipUnless(IsUnix, "Leading-dot hidden semantics only apply on Unix.");

        // Arrange - create file in hidden directory (Unix: starts with .)
        TempDir.CreateFile(".hidden/test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert - hidden directories are skipped and the file is left untouched
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.UnformattedKql, TempDir.ReadFile(".hidden/test.kql"));
    }

    [Fact]
    public async Task EdgeCase_FileWithDotInName_ProcessedCorrectly()
    {
        // Arrange
        TempDir.CreateFile("test.query.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("test.query.kql"));
    }

    [Fact]
    public async Task EdgeCase_KqlExtensionCaseInsensitive_AllProcessed()
    {
        // Arrange - .kql matching is case-insensitive on all platforms
        TempDir.CreateFile("lower.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("upper.KQL", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("mixed.Kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("lower.kql"));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("upper.KQL"));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("mixed.Kql"));
    }

    [Fact]
    public async Task EdgeCase_VeryLongFileName_ProcessedCorrectly()
    {
        // Arrange - create file with long name (but within OS limits)
        var longName = new string('a', 100) + ".kql";
        TempDir.CreateFile(longName, TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile(longName));
    }

    [Fact]
    public async Task EdgeCase_CheckEmptyFile_ReturnsZero()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.EmptyKql);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert - empty file should be considered "formatted"
        Assert.Equal(0, result.ExitCode);
    }
}
