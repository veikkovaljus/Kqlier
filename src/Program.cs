using System.CommandLine;

namespace Kqlier;

internal static class Program
{
    private static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Tool for formatting .kql files");

        var formatCommand = new Command(
            "format",
            "Format a .kql file or directory (stdin when no path is given)"
        );
        var formatPath = new Argument<string>("path")
        {
            Description = "A .kql file or directory. Reads stdin when omitted.",
            Arity = ArgumentArity.ZeroOrOne,
        };
        formatCommand.Add(formatPath);
        formatCommand.SetAction(parseResult =>
        {
            var path = parseResult.GetValue(formatPath);
            return Invoke(() => Kqlier.Format(path));
        });

        var checkCommand = new Command(
            "check",
            "Check if .kql files are formatted (exit 1 if not)"
        );
        var checkPath = new Argument<string>("path")
        {
            Description = "A .kql file or directory. Checks stdin when omitted.",
            Arity = ArgumentArity.ZeroOrOne,
        };
        checkCommand.Add(checkPath);
        checkCommand.SetAction(parseResult =>
        {
            var path = parseResult.GetValue(checkPath);
            return Invoke(() => Kqlier.Check(path));
        });

        rootCommand.Add(formatCommand);
        rootCommand.Add(checkCommand);

        var parseResult = rootCommand.Parse(args);
        var exitCode = parseResult.Invoke();
        return parseResult.Errors.Count > 0 ? ExitCodes.Error : exitCode;
    }

    private static int Invoke(Func<int> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return ExitCodes.Error;
        }
    }
}
