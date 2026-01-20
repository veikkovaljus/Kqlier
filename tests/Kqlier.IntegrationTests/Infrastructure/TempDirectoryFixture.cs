using System.Text;

namespace Kqlier.IntegrationTests.Infrastructure;

public sealed class TempDirectoryFixture : IDisposable
{
    public string RootPath { get; }

    public TempDirectoryFixture()
    {
        RootPath = Path.Combine(Path.GetTempPath(), "KqlierTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(RootPath);
    }

    public string CreateFile(string relativePath, string content, Encoding? encoding = null)
    {
        var fullPath = Path.Combine(RootPath, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        encoding ??= new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        File.WriteAllText(fullPath, content, encoding);
        return fullPath;
    }

    public string CreateFileBytes(string relativePath, byte[] bytes)
    {
        var fullPath = Path.Combine(RootPath, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(fullPath, bytes);
        return fullPath;
    }

    public byte[] ReadFileBytes(string relativePath)
    {
        return File.ReadAllBytes(Path.Combine(RootPath, relativePath));
    }

    public string ReadFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(RootPath, relativePath));
    }

    public DateTime GetLastWriteTime(string relativePath)
    {
        return File.GetLastWriteTimeUtc(Path.Combine(RootPath, relativePath));
    }

    public void SetFileMode(string relativePath, UnixFileMode mode)
    {
        var fullPath = Path.Combine(RootPath, relativePath);
        File.SetUnixFileMode(fullPath, mode);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(RootPath))
            {
                // Restore permissions on all files to allow deletion
                foreach (
                    var file in Directory.EnumerateFiles(RootPath, "*", SearchOption.AllDirectories)
                )
                {
                    try
                    {
                        File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                    }
                    catch
                    {
                        // Ignore permission errors during cleanup
                    }
                }

                Directory.Delete(RootPath, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }
}
