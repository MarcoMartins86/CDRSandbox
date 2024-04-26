﻿using System.Text.RegularExpressions;
using CDRSandbox.Services.Models.ValueObjects.Base;

namespace CDRSandbox.Services.Models.ValueObjects;

public class Phone : ValueObject
{
    private const string ValidationPattern = "^\\+?[0-9 ]{0,32}$";
    private static readonly Regex ValidationRegex = new(ValidationPattern);
    public string Number { get; }

    public Phone(string number)
    {
        if (!ValidationRegex.IsMatch(number))
            throw new Exception($"Invalid phone number format: {number}");

        Number = number;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }
}