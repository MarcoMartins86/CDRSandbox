using System.Globalization;
using CDRSandbox.Services.Models;

namespace CDRSandbox.Helpers;

public static class DateTimeHelper
{
    public static bool TryParseDate(string value, out DateTime date)
    {
        return DateTime.TryParseExact(value, CdrItem.CallDateFormat, DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.AssumeUniversal, out date);
    }
}