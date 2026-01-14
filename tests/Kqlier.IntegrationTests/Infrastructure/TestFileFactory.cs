using System.Text;

namespace Kqlier.IntegrationTests.Infrastructure;

public static class TestFileFactory
{
    // Sample unformatted KQL - the Kusto formatter will add spaces around operators
    public const string UnformattedKql =
        "StormEvents\n| where State==\"TEXAS\"\n| project StartTime,EndTime";

    // Expected formatted output (Kusto formatter adds spaces around operators and after commas)
    public const string FormattedKql =
        "StormEvents\n| where State == \"TEXAS\"\n| project StartTime, EndTime";

    // Simple already-formatted KQL
    public const string AlreadyFormattedKql = "StormEvents\n| where State == \"TEXAS\"";

    // Empty content
    public const string EmptyKql = "";

    // Whitespace only
    public const string WhitespaceOnlyKql = "   \n\t\n   ";

    public static byte[] CreateWithLf(string content)
    {
        var normalized = content.Replace("\r\n", "\n");
        return Encoding.UTF8.GetBytes(normalized);
    }

    public static byte[] CreateWithCrlf(string content)
    {
        var normalized = content.Replace("\r\n", "\n").Replace("\n", "\r\n");
        return Encoding.UTF8.GetBytes(normalized);
    }

    public static byte[] CreateWithUtf8Bom(string content)
    {
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var result = new byte[bom.Length + contentBytes.Length];
        bom.CopyTo(result, 0);
        contentBytes.CopyTo(result, bom.Length);
        return result;
    }

    public static byte[] CreateWithUtf16Le(string content)
    {
        var encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
        using var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, encoding, leaveOpen: true))
        {
            writer.Write(content);
        }
        return ms.ToArray();
    }

    public static byte[] CreateWithUtf16Be(string content)
    {
        var encoding = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
        using var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, encoding, leaveOpen: true))
        {
            writer.Write(content);
        }
        return ms.ToArray();
    }
}

public static class ByteHelpers
{
    public static bool HasUtf8Bom(byte[] bytes) =>
        bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;

    public static bool HasUtf16LeBom(byte[] bytes) =>
        bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE;

    public static bool HasUtf16BeBom(byte[] bytes) =>
        bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF;

    public static bool ContainsCrlf(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length - 1; i++)
        {
            if (bytes[i] == 0x0D && bytes[i + 1] == 0x0A)
                return true;
        }
        return false;
    }
}
