using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class LineEndingTests : KqlierTestBase
{
    [Fact]
    public async Task LineEndings_LF_PreservedAfterFormat()
    {
        // Arrange
        var bytes = TestFileFactory.CreateWithLf(TestFileFactory.UnformattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);
        Assert.False(ByteHelpers.ContainsCrlf(bytes));

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.False(ByteHelpers.ContainsCrlf(newBytes));
    }

    [Fact]
    public async Task LineEndings_CRLF_PreservedAfterFormat()
    {
        // Arrange
        var bytes = TestFileFactory.CreateWithCrlf(TestFileFactory.UnformattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);
        Assert.True(ByteHelpers.ContainsCrlf(bytes));

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.True(ByteHelpers.ContainsCrlf(newBytes));
    }

    [Fact]
    public async Task LineEndings_CheckMode_LF_FormattedFile_ReturnsZero()
    {
        // Arrange - create formatted file with LF
        var bytes = TestFileFactory.CreateWithLf(TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);

        // Act
        var result = await RunKqlierAsync("--check");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task LineEndings_CheckMode_CRLF_FormattedFile_ReturnsZero()
    {
        // Arrange - create formatted file with CRLF
        var bytes = TestFileFactory.CreateWithCrlf(TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);

        // Act
        var result = await RunKqlierAsync("--check");

        // Assert - formatted content should pass check regardless of line endings
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task LineEndings_NoTrailingNewline_HandledCorrectly()
    {
        // Arrange - file without trailing newline
        var content = "StormEvents\n| where State==\"TEXAS\"";
        var bytes = TestFileFactory.CreateWithLf(content);
        TempDir.CreateFileBytes("test.kql", bytes);

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task LineEndings_TrailingNewline_Preserved()
    {
        // Arrange - file with trailing newline
        var content = "StormEvents\n| where State==\"TEXAS\"\n";
        var bytes = TestFileFactory.CreateWithLf(content);
        TempDir.CreateFileBytes("test.kql", bytes);
        var hadTrailingNewline = bytes[^1] == 0x0A;

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        var hasTrailingNewline = newBytes.Length > 0 && newBytes[^1] == 0x0A;
        Assert.Equal(hadTrailingNewline, hasTrailingNewline);
    }

    [Fact]
    public async Task LineEndings_SingleLineFile_NoNewlines()
    {
        // Arrange - single line without newlines
        var content = "StormEvents | where State==\"TEXAS\"";
        TempDir.CreateFile("test.kql", content);

        // Act
        var result = await RunKqlierAsync();

        // Assert
        Assert.Equal(0, result.ExitCode);
    }
}
