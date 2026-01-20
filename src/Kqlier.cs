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
        var files = GetFiles().ToList();
        var modified = 0;
        foreach (var file in files)
        {
            if (FormatFile(file))
                modified++;
        }
        Console.WriteLine($"Formatted {modified} of {files.Count} files");
        return 0;
    }

    private static int Format(FileInfo file)
    {
        var wasModified = FormatFile(file);
        Console.WriteLine(
            wasModified ? $"Formatted {file.Name}" : $"{file.Name} already formatted"
        );
        return 0;
    }

    private static int Check()
    {
        var files = GetFiles().ToList();
        var failedFiles = new List<FileInfo>();

        foreach (var file in files)
        {
            if (CheckFile(file) != 0)
                failedFiles.Add(file);
        }

        Console.WriteLine($"Checked {files.Count} files, {failedFiles.Count} need formatting");

        if (failedFiles.Count > 0)
        {
            Console.WriteLine("Files not formatted:");
            foreach (var file in failedFiles)
                Console.WriteLine($"{file.FullName}");
        }

        return failedFiles.Count > 0 ? 1 : 0;
    }

    private static int Check(FileInfo file)
    {
        var needsFormatting = CheckFile(file) != 0;
        Console.WriteLine(
            needsFormatting ? $"{file.Name} needs formatting" : $"{file.Name} is formatted"
        );
        return needsFormatting ? 1 : 0;
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

    private static bool FormatFile(FileInfo file)
    {
        var (originalText, encoding) = ReadFile(file);
        var formattedText = FormatKql(originalText);

        if (!string.Equals(originalText, formattedText, StringComparison.Ordinal))
        {
            WriteFile(file, formattedText, encoding);
            return true;
        }
        return false;
    }

    private static string FormatKql(string text)
    {
        var kustoCode = KustoCode.Parse(text);
        var kustoCodeService = new KustoCodeService(kustoCode);
        return kustoCodeService.GetFormattedText().Text;
    }

    private static (string Text, Encoding Encoding) ReadFile(FileInfo file)
    {
        var bytes = File.ReadAllBytes(file.FullName);

        // Check for UTF-16 BE BOM (FE FF)
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            var encoding = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
            return (encoding.GetString(bytes, 2, bytes.Length - 2), encoding);
        }

        // Check for UTF-16 LE BOM (FF FE)
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            var encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
            return (encoding.GetString(bytes, 2, bytes.Length - 2), encoding);
        }

        // Check for UTF-8 BOM (EF BB BF)
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return (encoding.GetString(bytes, 3, bytes.Length - 3), encoding);
        }

        // Default: UTF-8 without BOM
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        return (utf8NoBom.GetString(bytes), utf8NoBom);
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
