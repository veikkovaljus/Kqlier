using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class EncodingTests : KqlierTestBase
{
    [Fact]
    public async Task Encoding_Utf8NoBom_PreservedAfterFormat()
    {
        // Arrange
        var bytes = TestFileFactory.CreateWithLf(TestFileFactory.UnformattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);
        Assert.False(ByteHelpers.HasUtf8Bom(bytes));

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.False(ByteHelpers.HasUtf8Bom(newBytes));
    }

    [Fact]
    public async Task Encoding_Utf8WithBom_PreservedAfterFormat()
    {
        // Arrange
        var bytes = TestFileFactory.CreateWithUtf8Bom(TestFileFactory.UnformattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);
        Assert.True(ByteHelpers.HasUtf8Bom(bytes));

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.True(ByteHelpers.HasUtf8Bom(newBytes));
    }

    [Fact]
    public async Task Encoding_Utf16LE_PreservedAfterFormat()
    {
        // Arrange
        var bytes = TestFileFactory.CreateWithUtf16Le(TestFileFactory.UnformattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);
        Assert.True(ByteHelpers.HasUtf16LeBom(bytes));

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.True(ByteHelpers.HasUtf16LeBom(newBytes));
    }

    [Fact]
    public async Task Encoding_Utf16BE_PreservedAfterFormat()
    {
        // Arrange
        var bytes = TestFileFactory.CreateWithUtf16Be(TestFileFactory.UnformattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);
        Assert.True(ByteHelpers.HasUtf16BeBom(bytes));

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        var newBytes = TempDir.ReadFileBytes("test.kql");
        Assert.True(ByteHelpers.HasUtf16BeBom(newBytes));
    }

    [Fact]
    public async Task Encoding_CheckMode_Utf8WithBom_ComparesCorrectly()
    {
        // Arrange - create a formatted file with BOM
        var bytes = TestFileFactory.CreateWithUtf8Bom(TestFileFactory.AlreadyFormattedKql);
        TempDir.CreateFileBytes("test.kql", bytes);

        // Act
        var result = await RunKqlierAsync("check", ".");

        // Assert - should pass as file is already formatted
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task Encoding_SpecialCharacters_PreservedCorrectly()
    {
        // Arrange - KQL with Unicode characters
        var kqlWithUnicode = "StormEvents\n| where Description==\"Ümläut tëst\"\n| project Name";
        TempDir.CreateFile("test.kql", kqlWithUnicode);

        // Act
        var result = await RunKqlierAsync("format", ".");

        // Assert
        Assert.Equal(0, result.ExitCode);
        var content = TempDir.ReadFile("test.kql");
        Assert.Contains("Ümläut tëst", content);
    }
}
