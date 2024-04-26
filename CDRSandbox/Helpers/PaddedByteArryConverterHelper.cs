﻿using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace CDRSandbox.Helpers;

public class PaddedByteArrayConverterHelper : ByteArrayConverter
{
    private readonly byte _byteLength;

    public PaddedByteArrayConverterHelper() : this(ByteArrayConverterOptions.Hexadecimal | ByteArrayConverterOptions.HexInclude0x)
    {
    }
    
    public PaddedByteArrayConverterHelper(ByteArrayConverterOptions options) : base(options)
    {
        _byteLength =
            (options & ByteArrayConverterOptions.HexDashes) == ByteArrayConverterOptions.HexDashes
                ? (byte)3
                : (byte)2; // code taken from the base constructor
    }
    
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var paddedText = PadHexStringHelper.Pad(text, _byteLength); // Pad the string
            return base.ConvertFromString(paddedText, row, memberMapData); // call the original converter
        }
        // call the original converter
        return base.ConvertFromString(text, row, memberMapData);
    }
    
    public byte[] ConvertFromString(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var paddedText = PadHexStringHelper.Pad(text, _byteLength); // Pad the string
            // abuse the base method by call this with null since it's safe when text is not null
            return (byte[])base.ConvertFromString(paddedText, null, null);
        }

        throw new Exception("Text to convert cannot be null or empty.");
    }
}