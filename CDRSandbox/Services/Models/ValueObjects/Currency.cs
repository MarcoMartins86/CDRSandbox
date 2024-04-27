using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Currency : ValueObject
{
    // TODO: this should be improved to have the active currency codes list and rates in a configurable place 
    // TODO: add more if needed
    private static readonly Dictionary<string, double> ActiveCurrencyCodesConversionRateToDefaultCurrency = new()
    {
        ["AUD"] = 0.52279068d,
        ["EUR"] = 0.85589298d,
        ["CNY"] = 0.11046396d,
        ["GBP"] = 1d,
        ["JPY"] = 0.0050577936d,
        ["USD"] = 0.80045178d,
    };

    public static Currency DefaultCurrency = new("GBP");
    
    public string Code { get; }

    public Currency(string code)
    {
        if (!ActiveCurrencyCodesConversionRateToDefaultCurrency.ContainsKey(code))
            throw new Exception($"Currency code unknown: [{code}]");

        Code = code;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString()
    {
        return Code;
    }
}
