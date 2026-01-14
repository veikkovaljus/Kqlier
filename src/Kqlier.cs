using System.Text;
using Kusto.Language;
using Kusto.Language.Editor;

namespace Kqlier;

public static class Kqlier
{
    public static int Run(FileInfo? fileInfo, bool check)
    {
        if (check)
        {
            return fileInfo != null ? Check(fileInfo) : Check();
        }

        return fileInfo != null ? Format(fileInfo) : Format();
    }

    private static int Format()
    {
        var filesToFormat = GetFiles();

        var count = 0;
        foreach (var file in filesToFormat)
        {
            FormatFile(file);
            count++;
        }

        Console.WriteLine($"Formatted {count} files");
        return 0;
    }

    private static int Format(FileInfo file)
    {
        FormatFile(file);
        Console.WriteLine($"Formatted {file.Name}");
        return 0;
    }

    private static int Check()
    {
        var filesToFormat = GetFiles();
        return filesToFormat.Select(CheckFile).Any(result => result != 0) ? 1 : 0;
    }

    private static int Check(FileInfo file)
    {
        var result = CheckFile(file);
        Console.WriteLine($"Checked {file.Name}");
        return result;
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
        // Check for UTF-8 BOM before reading
        bool hasUtf8Bom = false;
        using (
            var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
        )
        {
            if (fs.Length >= 3)
            {
                var bom = new byte[3];
                fs.ReadExactly(bom, 0, 3);
                hasUtf8Bom = bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
            }
        }

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

        // If UTF-8 without BOM, use encoding that won't emit BOM
        if (encoding.CodePage == 65001 && !hasUtf8Bom)
        {
            encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        }

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
