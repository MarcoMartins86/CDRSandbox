using System.Text.RegularExpressions;
using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Phone : ValueObject
{
    private static readonly Regex ValidationRegex = new(CdrItem.PhoneNumberPattern);
    public string Number { get; }

    public Phone(string number)
    {
        if (!ValidationRegex.IsMatch(number))
            throw new Exception($"Invalid phone number format: [{number}]");

        Number = number;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }

    public override string ToString()
    {
        return Number;
    }
}