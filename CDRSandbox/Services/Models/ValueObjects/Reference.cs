using System.Text.RegularExpressions;
using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Reference : ValueObject
{
    private static readonly Regex ValidationRegex = new(CdrItem.ReferencePattern);
    
    private string Value { get; }

    public Reference(string reference)
    {
        if (reference == null || !ValidationRegex.IsMatch(reference))
            throw new Exception($"Invalid call detail record reference: [{reference}]");
        Value = reference;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}