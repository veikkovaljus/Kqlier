using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class FormatSingleFileTests : KqlierTestBase
{
    [Fact]
    public async Task FormatSingleFile_UnformattedFile_FormatsSuccessfully()
    {
        // Arrange
        var filePath = TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("--file", filePath);

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("test.kql"));
        Assert.Contains("Formatted test.kql", result.StandardOutput);
    }

    [Fact]
    public async Task FormatSingleFile_RelativePath_Works()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("--file", "test.kql");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("test.kql"));
    }

    [Fact]
    public async Task FormatSingleFile_FileInSubdirectory_FormatsCorrectly()
    {
        // Arrange
        var filePath = TempDir.CreateFile("subdir/test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("--file", filePath);

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("subdir/test.kql"));
    }

    [Fact]
    public async Task FormatSingleFile_OtherFilesUntouched_OnlyTargetModified()
    {
        // Arrange
        var targetPath = TempDir.CreateFile("target.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("other.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("--file", targetPath);

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("target.kql"));
        Assert.Equal(TestFileFactory.UnformattedKql, TempDir.ReadFile("other.kql"));
    }

    [Fact]
    public async Task FormatSingleFile_AlreadyFormatted_NoModification()
    {
        // Arrange
        var filePath = TempDir.CreateFile("test.kql", TestFileFactory.AlreadyFormattedKql);
        await Task.Delay(100);
        var originalTime = TempDir.GetLastWriteTime("test.kql");
        await Task.Delay(100);

        // Act
        var result = await RunKqlierAsync("--file", filePath);

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newTime = TempDir.GetLastWriteTime("test.kql");
        Assert.Equal(originalTime, newTime);
    }

    [Fact]
    public async Task FormatSingleFile_NonExistentFile_ReturnsError()
    {
        // Arrange
        var nonExistentPath = Path.Combine(TempDir.RootPath, "nonexistent.kql");

        // Act
        var result = await RunKqlierAsync("--file", nonExistentPath);

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }
}
