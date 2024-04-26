namespace CDRSandbox.Helpers;

public static class PadHexStringHelper
{
    public static string Pad(string text, int byteLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        // if the hex string needs to be padded we will add the "0" to the begging of it
       return Enumerable.Range(0, text.Length % byteLength)
            .Aggregate(text, (s, i) => text.Insert(0, "0"));
    }
    
}