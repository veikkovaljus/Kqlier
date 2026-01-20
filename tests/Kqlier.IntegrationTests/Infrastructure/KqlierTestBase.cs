namespace Kqlier.IntegrationTests.Infrastructure;

public abstract class KqlierTestBase : IDisposable
{
    protected TempDirectoryFixture TempDir { get; }

    protected KqlierTestBase()
    {
        TempDir = new TempDirectoryFixture();
    }

    protected Task<ProcessResult> RunKqlierAsync(params string[] args)
    {
        return ProcessRunner.RunKqlierAsync(TempDir.RootPath, args);
    }

    protected Task<ProcessResult> RunKqlierWithStdinAsync(string stdinInput, params string[] args)
    {
        return ProcessRunner.RunKqlierAsync(TempDir.RootPath, stdinInput, args);
    }

    public void Dispose()
    {
        TempDir.Dispose();
        GC.SuppressFinalize(this);
    }
}
