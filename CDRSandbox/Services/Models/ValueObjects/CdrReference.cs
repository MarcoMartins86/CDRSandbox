using CDRSandbox.Helpers;
using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class CdrReference : ValueObject
{
    private const int MaxBytes = 17;
    private const int CharsBytesLength = 2;
    private const int MaxChars = MaxBytes * CharsBytesLength;
    private static readonly PaddedByteArrayConverterHelper Converter = new ();
    
    public byte[] Value { get; }

    public CdrReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > MaxChars)
            throw new Exception($"Invalid Call Detail Record reference: {reference}");
        Value = Converter.ConvertFromString(reference);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}