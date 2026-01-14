using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class CheckSingleFileTests : KqlierTestBase
{
    [Fact]
    public async Task CheckSingleFile_Formatted_ReturnsZero()
    {
        // Arrange
        var filePath = TempDir.CreateFile("test.kql", TestFileFactory.AlreadyFormattedKql);

        // Act
        var result = await RunKqlierAsync("--check", "--file", filePath);

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Checked test.kql", result.StandardOutput);
    }

    [Fact]
    public async Task CheckSingleFile_Unformatted_ReturnsOne()
    {
        // Arrange
        var filePath = TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("--check", "--file", filePath);

        // Assert
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public async Task CheckSingleFile_DoesNotModifyFile()
    {
        // Arrange
        var filePath = TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);
        var originalBytes = TempDir.ReadFileBytes("test.kql");

        // Act
        var result = await RunKqlierAsync("--check", "--file", filePath);

        // Assert
        Assert.Equal(1, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.Equal(originalBytes, newBytes);
    }

    [Fact]
    public async Task CheckSingleFile_OtherFilesIgnored()
    {
        // Arrange - one formatted target, one unformatted other file
        var targetPath = TempDir.CreateFile("target.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("other.kql", TestFileFactory.UnformattedKql);

        // Act - check only the formatted target
        var result = await RunKqlierAsync("--check", "--file", targetPath);

        // Assert - should pass because we only check the target
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task CheckSingleFile_RelativePath_Works()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.AlreadyFormattedKql);

        // Act
        var result = await RunKqlierAsync("--check", "--file", "test.kql");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task CheckSingleFile_NonExistentFile_ReturnsError()
    {
        // Arrange
        var nonExistentPath = Path.Combine(TempDir.RootPath, "nonexistent.kql");

        // Act
        var result = await RunKqlierAsync("--check", "--file", nonExistentPath);

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }
}
