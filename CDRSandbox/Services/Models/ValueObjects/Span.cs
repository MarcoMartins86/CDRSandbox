using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Span(uint seconds) : ValueObject
{
    private readonly TimeSpan _value = TimeSpan.FromSeconds(seconds);
    public uint TotalSeconds => (uint)_value.TotalSeconds;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return _value;
    }

    public override string ToString()
    {
        return ((uint)_value.TotalSeconds).ToString();
    }
}