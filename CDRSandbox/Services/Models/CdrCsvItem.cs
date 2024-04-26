using CsvHelper.Configuration.Attributes;

namespace CDRSandbox.Services.Models;

[Delimiter(",")]
[CultureInfo("InvariantCulture")]
[CountBytes(false)]
[HasHeaderRecord(true)]
[IgnoreBlankLines(true)]
public class CdrCsvItem
{
    [Name("caller_id")]
    public string? CallerId { get; set; }
    
    [Name("recipient")]
    public string? Recipient { get; set; }
    
    [Name("call_date")]
    [Format(["dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy"])]
    public DateOnly? CallDate { get; set; }
    
    [Name("end_time")]
    public TimeOnly? EndTime { get; set; }
    
    [Name("duration")]
    public int? Duration { get; set; }
    
    [Name("cost")]
    public float? Cost { get; set; }
    
    [Name("reference")]
    public byte[]? Reference { get; set; }
    
    [Name("currency")]
    public string? Currency { get; set; }
    
    [Optional]
    [Name("type")]
    public CdrCallTypeEnum? Type { get; set; }

    // must be on the same order than CdrRepositoryClickhouseImpl.ColumnsName
    public object?[] ToObjects() => [CallerId, Recipient, CallDate, EndTime, Duration, Cost, Reference, Currency, Type];
}