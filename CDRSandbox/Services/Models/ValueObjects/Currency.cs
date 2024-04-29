using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Currency : ValueObject
{
    // TODO: this should be improved to have the active currency codes list and rates in a configurable place 
    // TODO: add more if needed
    public static readonly string[] ActiveCurrencyArray = ["AUD", "EUR", "CNY", "GBP", "JPY", "USD"];
    private static readonly HashSet<string> ActiveCurrencySet = [..ActiveCurrencyArray];
    
    private string Code { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || !ActiveCurrencySet.Contains(code))
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
