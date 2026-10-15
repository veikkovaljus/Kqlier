using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class CheckAllFilesTests : KqlierTestBase
{
    [Fact]
    public async Task CheckAllFiles_AllFormatted_ReturnsZero()
    {
        // Arrange
        TempDir.CreateFile("file1.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("file2.kql", TestFileFactory.AlreadyFormattedKql);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task CheckAllFiles_OneUnformatted_ReturnsOne()
    {
        // Arrange
        TempDir.CreateFile("formatted.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("unformatted.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public async Task CheckAllFiles_AllUnformatted_ReturnsOne()
    {
        // Arrange
        TempDir.CreateFile("file1.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("file2.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("file3.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public async Task CheckAllFiles_EmptyDirectory_ReturnsZero()
    {
        // Arrange - empty directory

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task CheckAllFiles_DoesNotModifyFiles()
    {
        // Arrange
        var originalContent = TestFileFactory.UnformattedKql;
        TempDir.CreateFile("test.kql", originalContent);
        var originalBytes = TempDir.ReadFileBytes("test.kql");

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(1, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.Equal(originalBytes, newBytes);
    }

    [Fact]
    public async Task CheckAllFiles_RecursiveDiscovery_ChecksSubdirectories()
    {
        // Arrange - all formatted
        TempDir.CreateFile("root.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("sub1/file1.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("sub1/sub2/file2.kql", TestFileFactory.AlreadyFormattedKql);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task CheckAllFiles_UnformattedInSubdirectory_ReturnsOne()
    {
        // Arrange - unformatted file deep in subdirectory
        TempDir.CreateFile("root.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("sub1/file1.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("sub1/sub2/unformatted.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public async Task CheckAllFiles_TargetDirectory_ChecksOnlyThatDirectory()
    {
        // Arrange - unformatted file in target dir, formatted file elsewhere
        TempDir.CreateFile("target/file.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("other/file.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("check", "target");

        // Assert - only the target dir is checked, which has one unformatted file
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Checked 1 files, 1 need formatting", result.StandardOutput);
    }
}
