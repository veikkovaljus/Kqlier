using System.CommandLine;
using Kusto.Language;
using Kusto.Language.Editor;

namespace Kqlier;

internal static class Kqlier
{
    private static int Main(string[] args)
    {
        Option<FileInfo> fileOption = new("--file") { Description = "The file to format" };
        Option<bool> checkOption = new("--check")
        {
            Description = "Checks if all files all formatted",
        };

        RootCommand rootCommand = new("Tool for formatting .kql files");
        rootCommand.Options.Add(fileOption);
        rootCommand.Options.Add(checkOption);

        rootCommand.SetAction(parseResult =>
        {
            var fileInfo = parseResult.GetValue(fileOption);
            if (fileInfo != null)
            {
                throw new NotImplementedException();
            }

            var checkInfo = parseResult.GetValue(checkOption);
            if (checkInfo)
            {
                throw new NotImplementedException();
            }

            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var files = directoryInfo
                .EnumerateFiles("*.kql", SearchOption.AllDirectories)
                .ToArray();

            foreach (var file in files)
            {
                FormatFile(file);
            }

            return 0;
        });

        var parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    private static void FormatFile(FileInfo file)
    {
        var fileContent = File.ReadAllText(file.FullName);
        var formattedKql = FormatKql(fileContent);
        if (fileContent != formattedKql)
        {
            WriteToFile(file, formattedKql);
        }
    }

    private static void WriteToFile(FileInfo file, string formattedKql)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, formattedKql);
        File.Move(tempFile, file.FullName, overwrite: true);
    }

    private static string FormatKql(string text)
    {
        var kustoCode = KustoCode.Parse(text);
        var kustoCodeService = new KustoCodeService(kustoCode);
        return kustoCodeService.GetFormattedText().Text;
    }
}
