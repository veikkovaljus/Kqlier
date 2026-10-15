using System.Reflection;
using Kqlier.IntegrationTests.Infrastructure;
using Xunit;

namespace Kqlier.IntegrationTests.Tests;

public class VersionTests : KqlierTestBase
{
    [Fact]
    public async Task Version_Flag_PrintsAssemblyVersion()
    {
        var expected = typeof(global::Kqlier.Kqlier)
            .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;

        var result = await RunKqlierAsync("--version");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expected, result.StandardOutput.Trim());
    }
}
