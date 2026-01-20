using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class StdinTests : KqlierTestBase
{
    [Fact]
    public async Task Stdin_UnformattedInput_ReturnsFormattedOutput()
    {
        // Arrange
        var input = "StormEvents\n| where State==\"TEXAS\"\n| project StartTime,EndTime";

        // Act
        var result = await RunKqlierWithStdinAsync(input, "--stdin");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("State == \"TEXAS\"", result.StandardOutput);
        Assert.Contains("StartTime, EndTime", result.StandardOutput);
    }

    [Fact]
    public async Task Stdin_AlreadyFormattedInput_ReturnsSameOutput()
    {
        // Arrange
        var input = "StormEvents\n| where State == \"TEXAS\"";

        // Act
        var result = await RunKqlierWithStdinAsync(input, "--stdin");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("State == \"TEXAS\"", result.StandardOutput);
    }

    [Fact]
    public async Task Stdin_EmptyInput_ReturnsEmptyOutput()
    {
        // Arrange
        var input = "";

        // Act
        var result = await RunKqlierWithStdinAsync(input, "--stdin");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task StdinCheck_UnformattedInput_ReturnsOne()
    {
        // Arrange
        var input = "StormEvents\n| where State==\"TEXAS\"";

        // Act
        var result = await RunKqlierWithStdinAsync(input, "--stdin", "--check");

        // Assert
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Input needs formatting", result.StandardError);
    }

    [Fact]
    public async Task StdinCheck_FormattedInput_ReturnsZero()
    {
        // Arrange
        var input = "StormEvents\n| where State == \"TEXAS\"";

        // Act
        var result = await RunKqlierWithStdinAsync(input, "--stdin", "--check");

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Input is formatted", result.StandardError);
    }

    [Fact]
    public async Task StdinCheck_StatusGoesToStderr()
    {
        // Arrange
        var input = "StormEvents\n| where State==\"TEXAS\"";

        // Act
        var result = await RunKqlierWithStdinAsync(input, "--stdin", "--check");

        // Assert - status message should be in stderr, not stdout
        Assert.Contains("Input needs formatting", result.StandardError);
        Assert.DoesNotContain("Input needs formatting", result.StandardOutput);
    }
}
