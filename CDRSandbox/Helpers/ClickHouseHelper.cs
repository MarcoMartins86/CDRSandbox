namespace CDRSandbox.Helpers;

public static class ClickHouseHelper
{
    public static string UnpadFixedString(string value)
    {
        // FixedStrings in ClickHouse pads them with '\0' at the end, so we need to treat them
        var index = value.IndexOf('\0');
        return index != -1 ? value.Substring(0, index) : value;
    }
}