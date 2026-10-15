using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class FileWriteTests : KqlierTestBase
{
    private static bool IsUnix => OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();

    [Fact]
    public async Task Format_PreservesUnixFileMode()
    {
        Assert.SkipUnless(IsUnix, "Unix file modes only apply on Unix.");

        // Arrange - a file with the typical 0644 mode
        TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);
        TempDir.SetFileMode(
            "test.kql",
            UnixFileMode.UserRead
                | UnixFileMode.UserWrite
                | UnixFileMode.GroupRead
                | UnixFileMode.OtherRead
        );

        // Act
        var result = await RunKqlierAsync("format", "test.kql");

        // Assert - mode preserved (regression guard for the 0644 -> 0600 fix)
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(
            UnixFileMode.UserRead
                | UnixFileMode.UserWrite
                | UnixFileMode.GroupRead
                | UnixFileMode.OtherRead,
            TempDir.GetFileMode("test.kql")
        );
    }

    [Fact]
    public async Task Format_PreservesWindowsFileAttributes()
    {
        Assert.SkipUnless(
            OperatingSystem.IsWindows(),
            "Windows file attributes only apply on Windows."
        );

        // Arrange
        var path = TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);
        File.SetAttributes(path, FileAttributes.Hidden);

        // Act
        var result = await RunKqlierAsync("format", "test.kql");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.True(File.GetAttributes(path).HasFlag(FileAttributes.Hidden));
    }

    [Fact]
    public async Task Format_LeavesNoTemporaryFiles()
    {
        // Arrange
        TempDir.CreateFile("test.kql", TestFileFactory.UnformattedKql);

        // Act
        var result = await RunKqlierAsync("format", "test.kql");

        // Assert - the atomic temp file is removed after the move
        Assert.Equal(0, result.ExitCode);
        var files = Directory.GetFiles(
            TempDir.RootPath,
            "*",
            new EnumerationOptions { AttributesToSkip = FileAttributes.None }
        );
        Assert.Single(files);
        Assert.DoesNotContain(files, file => file.EndsWith(".tmp", StringComparison.Ordinal));
        Assert.Equal(TestFileFactory.FormattedKql, TempDir.ReadFile("test.kql"));
    }
}
