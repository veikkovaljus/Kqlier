using System.Text;
using Kusto.Language;
using Kusto.Language.Editor;

namespace Kqlier;

public static class Kqlier
{
    public static int Format()
    {
        var filesToFormat = GetFiles();

        foreach (var file in filesToFormat)
        {
            FormatFile(file);
        }
        return 0;
    }

    public static int Format(FileInfo file)
    {
        FormatFile(file);
        return 0;
    }

    public static int Check()
    {
        var filesToFormat = GetFiles();

        return filesToFormat.Select(CheckFile).Any(result => result != 0) ? 1 : 0;
    }

    public static int Check(FileInfo file)
    {
        return CheckFile(file);
    }

    private static IEnumerable<FileInfo> GetFiles()
    {
        var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
        return directoryInfo.EnumerateFiles("*.kql", SearchOption.AllDirectories);
    }

    private static int CheckFile(FileInfo file)
    {
        var (originalText, _) = ReadFile(file);
        var formattedText = FormatKql(originalText);

        return !string.Equals(originalText, formattedText, StringComparison.Ordinal) ? 1 : 0;
    }

    private static void FormatFile(FileInfo file)
    {
        var (originalText, encoding) = ReadFile(file);
        var formattedText = FormatKql(originalText);

        if (!string.Equals(originalText, formattedText, StringComparison.Ordinal))
        {
            WriteFile(file, formattedText, encoding);
        }
    }

    private static string FormatKql(string text)
    {
        var kustoCode = KustoCode.Parse(text);
        var kustoCodeService = new KustoCodeService(kustoCode);
        return kustoCodeService.GetFormattedText().Text;
    }

    private static (string Text, Encoding Encoding) ReadFile(FileInfo file)
    {
        using var stream = new FileStream(
            file.FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        using var reader = new StreamReader(
            stream,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true
        );

        var text = reader.ReadToEnd();
        var encoding = reader.CurrentEncoding;

        return (text, encoding);
    }

    private static void WriteFile(FileInfo file, string text, Encoding encoding)
    {
        var tempFile = Path.GetTempFileName();

        using (
            var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None)
        )
        using (var writer = new StreamWriter(stream, encoding))
        {
            writer.Write(text);
        }

        var attributes = File.GetAttributes(file.FullName);
        File.Move(tempFile, file.FullName, overwrite: true);
        File.SetAttributes(file.FullName, attributes);
    }
}
