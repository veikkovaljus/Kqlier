using System.Runtime.InteropServices;
using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class ErrorHandlingTests : KqlierTestBase
{
    private static bool IsUnix =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    [Fact]
    public async Task Format_NonExistentFile_ReturnsErrorWithMessage()
    {
        // Arrange
        var nonExistentPath = Path.Combine(TempDir.RootPath, "nonexistent.kql");

        // Act
        var result = await RunKqlierAsync("format", nonExistentPath);

        // Assert
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("is not an existing file or directory", result.StandardError);
    }

    [Fact]
    public async Task Check_NonExistentFile_ReturnsErrorWithMessage()
    {
        // Arrange
        var nonExistentPath = Path.Combine(TempDir.RootPath, "nonexistent.kql");

        // Act
        var result = await RunKqlierAsync("check", nonExistentPath);

        // Assert
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("is not an existing file or directory", result.StandardError);
    }

    [Fact]
    [Trait("Category", "Unix")]
    public async Task FormatAll_UnreadableFile_ContinuesAndReportsError()
    {
        if (!IsUnix)
        {
            return; // Skip on non-Unix platforms
        }

        // Arrange
        TempDir.CreateFile("readable.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("unreadable.kql", TestFileFactory.UnformattedKql);
        TempDir.SetFileMode("unreadable.kql", UnixFileMode.None);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("Formatted 1 of 2 files", result.StandardOutput);
        Assert.Contains("Errors occurred in 1 file(s):", result.StandardError);
        Assert.Contains("unreadable.kql", result.StandardError);
        // Verify the readable file was still formatted
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("readable.kql"));
    }

    [Fact]
    [Trait("Category", "Unix")]
    public async Task CheckAll_UnreadableFile_ContinuesAndReportsError()
    {
        if (!IsUnix)
        {
            return; // Skip on non-Unix platforms
        }

        // Arrange
        TempDir.CreateFile("readable.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("unreadable.kql", TestFileFactory.UnformattedKql);
        TempDir.SetFileMode("unreadable.kql", UnixFileMode.None);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("Checked 2 files", result.StandardOutput);
        Assert.Contains("Errors occurred in 1 file(s):", result.StandardError);
        Assert.Contains("unreadable.kql", result.StandardError);
    }

    [Fact]
    [Trait("Category", "Unix")]
    public async Task FormatAll_MultipleUnreadableFiles_ReportsAllErrors()
    {
        if (!IsUnix)
        {
            return; // Skip on non-Unix platforms
        }

        // Arrange
        TempDir.CreateFile("readable.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("unreadable1.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("unreadable2.kql", TestFileFactory.UnformattedKql);
        TempDir.SetFileMode("unreadable1.kql", UnixFileMode.None);
        TempDir.SetFileMode("unreadable2.kql", UnixFileMode.None);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("Formatted 1 of 3 files", result.StandardOutput);
        Assert.Contains("Errors occurred in 2 file(s):", result.StandardError);
        Assert.Contains("unreadable1.kql", result.StandardError);
        Assert.Contains("unreadable2.kql", result.StandardError);
    }

    [Fact]
    [Trait("Category", "Unix")]
    public async Task FormatAll_AllFilesUnreadable_ReportsAllErrors()
    {
        if (!IsUnix)
        {
            return; // Skip on non-Unix platforms
        }

        // Arrange
        TempDir.CreateFile("unreadable1.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("unreadable2.kql", TestFileFactory.UnformattedKql);
        TempDir.SetFileMode("unreadable1.kql", UnixFileMode.None);
        TempDir.SetFileMode("unreadable2.kql", UnixFileMode.None);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("Formatted 0 of 2 files", result.StandardOutput);
        Assert.Contains("Errors occurred in 2 file(s):", result.StandardError);
    }

    [Fact]
    public async Task FormatAll_NoErrors_ReturnsZeroExitCode()
    {
        // Arrange
        TempDir.CreateFile("file1.kql", TestFileFactory.UnformattedKql);
        TempDir.CreateFile("file2.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Formatted 2 of 2 files", result.StandardOutput);
        Assert.DoesNotContain("Errors occurred", result.StandardError);
    }

    [Fact]
    public async Task CheckAll_NoErrors_ReturnsZeroExitCode()
    {
        // Arrange
        TempDir.CreateFile("file1.kql", TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFile("file2.kql", TestFileFactory.AlreadyFormattedKql);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Checked 2 files, 0 need formatting", result.StandardOutput);
        Assert.DoesNotContain("Errors occurred", result.StandardError);
    }
}
