using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class FormatAllFilesTests : KqlierTestBase
{
    [Fact]
    public async Task FormatAllFiles_SingleFile_FormatsSuccessfully()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        var content = TempDir.ReadFile("test.kql");
        Assert.Equal(TestFileFactory.FormattedKql, content);
    }

    [Fact]
    public async Task FormatAllFiles_MultipleFiles_FormatsAll()
    {
        // Arrange
        TempDir.CreateFile("file1.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("file2.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("file3.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("file1.kql"));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("file2.kql"));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("file3.kql"));
        Assert.Contains("Formatted 3 of 3 files", result.StandardOutput);
    }

    [Fact]
    public async Task FormatAllFiles_RecursiveDiscovery_FormatsFilesInSubdirectories()
    {
        // Arrange
        TempDir.CreateFile("root.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("sub1/file1.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("sub1/sub2/file2.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("root.kql"));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("sub1/file1.kql"));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("sub1/sub2/file2.kql"));
        Assert.Contains("Formatted 3 of 3 files", result.StandardOutput);
    }

    [Fact]
    public async Task FormatAllFiles_EmptyDirectory_ReturnsSuccessWithZeroFiles()
    {
        // Arrange - empty directory (already done by fixture)

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Formatted 0 of 0 files", result.StandardOutput);
    }

    [Fact]
    public async Task FormatAllFiles_NonKqlFiles_IgnoresOtherExtensions()
    {
        // Arrange
        TempDir.CreateFile("test.txt", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("test.sql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("test.kusto", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Formatted 0 of 0 files", result.StandardOutput);
        // Verify files are unchanged
        Assert.Equal(TestFileFactory.UnformattedKql, TempDir.ReadFile("test.txt"));
        Assert.Equal(TestFileFactory.UnformattedKql, TempDir.ReadFile("test.sql"));
        Assert.Equal(TestFileFactory.UnformattedKql, TempDir.ReadFile("test.kusto"));
    }

    [Fact]
    public async Task FormatAllFiles_AlreadyFormatted_DoesNotModifyTimestamp()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.AlreadyFormattedKql);
        await Task.Delay(100, TestContext.Current.CancellationToken); // Ensure timestamp resolution
        var originalTime = TempDir.GetLastWriteTime("test.kql");
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newTime = TempDir.GetLastWriteTime("test.kql");
        Assert.Equal(originalTime, newTime);
    }

    [Fact]
    public async Task FormatAllFiles_MixedFormattedAndUnformatted_OnlyModifiesUnformatted()
    {
        // Arrange
        TempDir.CreateFile("formatted.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("unformatted.kql", TestFileFactory.UnformattedKql);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        var formattedTime = TempDir.GetLastWriteTime("formatted.kql");
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(formattedTime, TempDir.GetLastWriteTime("formatted.kql"));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("unformatted.kql"));
    }

    [Fact]
    public async Task FormatAllFiles_TargetDirectory_FormatsOnlyThatDirectory()
    {
        // Arrange
        TempDir.CreateFile("target/file.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("other/file.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", "target");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("target/file.kql"));
        Assert.Equal(TestFileFactory.UnformattedKql, TempDir.ReadFile("other/file.kql"));
    }
}
