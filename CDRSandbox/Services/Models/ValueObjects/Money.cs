using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Money(double value, string currency) : ValueObject
{
    public double Value { get; } = value;
    public Currency Currency { get; } = new(currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Currency;
    }
}