using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Money(double value, string currency) : ValueObject
{
    private const int DefaultDecimalPlaces = 3;

    public float Amount => (float)Math.Round(value, DefaultDecimalPlaces, MidpointRounding.ToEven);
    public Currency Currency { get; } = new(currency); // TODO: need to make constructor to exchange to GBP

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }
}