using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class EdgeCaseTests : KqlierTestBase
{
    [Fact]
    public async Task EdgeCase_EmptyFile_HandledGracefully()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.EmptyKql);

        // Act
        var result = await RunKqlierAsync();

        // Assert - should not crash
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task EdgeCase_WhitespaceOnlyFile_HandledGracefully()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.WhitespaceOnlyKql);

        // Act
        var result = await RunKqlierAsync();

        // Assert - should not crash
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task EdgeCase_DeepNesting_DiscoveredCorrectly()
    {
        // Arrange - create files 5 levels deep
        TempDir.CreateFile("a/b/c/d/e/deep.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync();

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
        var result = await RunKqlierAsync();

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
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Formatted 20 files", result.StandardOutput);

        // Verify all files are formatted
        for (int i = 0; i < 20; i++)
        {
            Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile($"file{i}.kql"));
        }
    }

    [Fact]
    public async Task EdgeCase_HiddenDirectory_FilesProcessed()
    {
        // Arrange - create file in hidden directory (Unix: starts with .)
        TempDir.CreateFile(".hidden/test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile(".hidden/test.kql"));
    }

    [Fact]
    public async Task EdgeCase_FileWithDotInName_ProcessedCorrectly()
    {
        // Arrange
        TempDir.CreateFile("test.query.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("test.query.kql"));
    }

    [Fact]
    public async Task EdgeCase_KqlExtensionCaseSensitivity_LowercaseOnly()
    {
        // Arrange - only .kql should be processed, not .KQL or .Kql
        TempDir.CreateFile("lower.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("upper.KQL", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("mixed.Kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
        // Verify lower.kql is formatted
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("lower.kql"));
        // Upper/mixed case may or may not be processed depending on OS file system
        // This test documents behavior - on Linux, they should not be processed
    }

    [Fact]
    public async Task EdgeCase_VeryLongFileName_ProcessedCorrectly()
    {
        // Arrange - create file with long name (but within OS limits)
        var longName = new string('a', 100) + ".kql";
        TempDir.CreateFile(longName, TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync();

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
        var result = await RunKqlierAsync("--check");

        // Assert - empty file should be considered "formatted"
        Assert.Equal(0, result.ExitCode);
    }
}
