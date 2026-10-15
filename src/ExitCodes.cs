namespace Kqlier;

internal static class ExitCodes
{
    /// <summary>All files were formatted or already formatted.</summary>
    public const int Success = 0;

    /// <summary>One or more files need formatting (check mode).</summary>
    public const int NeedsFormatting = 1;

    /// <summary>A runtime, I/O, or usage error occurred.</summary>
    public const int Error = 2;
}
