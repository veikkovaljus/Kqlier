using System.CommandLine;

namespace Kqlier;

internal static class Program
{
    private static int Main(string[] args)
    {
        Option<FileInfo> fileOption = new("--file") { Description = "The file to format" };
        Option<bool> checkOption = new("--check")
        {
            Description = "Checks if all files are formatted",
        };
        Option<bool> stdinOption = new("--stdin")
        {
            Description = "Read from stdin and write to stdout",
        };

        RootCommand rootCommand = new("Tool for formatting .kql files");
        rootCommand.Options.Add(fileOption);
        rootCommand.Options.Add(checkOption);
        rootCommand.Options.Add(stdinOption);

        rootCommand.SetAction(parseResult =>
        {
            var fileInfo = parseResult.GetValue(fileOption);
            var checkInfo = parseResult.GetValue(checkOption);
            var stdinInfo = parseResult.GetValue(stdinOption);
            return Kqlier.Run(fileInfo, checkInfo, stdinInfo);
        });
        var parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }
}
