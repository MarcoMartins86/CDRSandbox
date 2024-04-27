﻿using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Time(string time) : ValueObject
{
    public const string DefaultFormat = CdrItem.EndTimeFormat; // TODO
    public static readonly string[] AcceptedFormats = [CdrItem.EndTimeFormat]; // TODO

    public TimeOnly Value { get; } = TimeOnly.ParseExact(time, AcceptedFormats);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString(DefaultFormat);
    }
}