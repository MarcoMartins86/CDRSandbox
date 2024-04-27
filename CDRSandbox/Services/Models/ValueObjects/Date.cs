using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Date(string date) : ValueObject
{
    public const string DefaultFormat = CdrItem.CallDateFormat; // TODO
    public static readonly string[] AcceptedFormats = [CdrItem.CallDateFormat]; // TODO

    public DateOnly Value { get; } = DateOnly.ParseExact(date, AcceptedFormats);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString(DefaultFormat);
    }
}