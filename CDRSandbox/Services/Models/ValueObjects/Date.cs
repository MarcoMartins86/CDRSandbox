using System.Globalization;
using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Date : ValueObject
{
    public const string DefaultFormat = CdrItem.CallDateFormat; // TODO
    public static readonly string[] AcceptedFormats = [CdrItem.CallDateFormat]; // TODO

    private DateOnly Value { get; }

    public Date(string date)
    {
        if (!DateOnly.TryParseExact(date, AcceptedFormats, DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None, out var dateOnly))
            throw new Exception($"Date in and unknown format: [{date}]. Accepted formats: [{string.Join(',', AcceptedFormats)}]");

        Value = dateOnly;
    }
    
    public Date(DateOnly date)
    {
        Value = date;
    }
    
    public Date(DateTime date)
    {
        Value = DateOnly.FromDateTime(date);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString(DefaultFormat);

    public DateTime ToDateTime() => Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    
    public DateOnly ToDateOnly() => Value;
    
    public static bool operator ==(Date one, DateTime two)
    {
        return one.Value == DateOnly.FromDateTime(two);
    }
    
    public static bool operator !=(Date one, DateTime two)
    {
        return one.Value != DateOnly.FromDateTime(two);
    }
}