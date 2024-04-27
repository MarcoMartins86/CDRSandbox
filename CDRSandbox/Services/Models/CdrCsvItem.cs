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
    [Format("HH:mm:ss")]
    public TimeOnly? EndTime { get; set; }
    
    [Name("duration")]
    public uint? Duration { get; set; }
    
    [Name("cost")]
    public float? Cost { get; set; }
    
    [Name("reference")]
    public string? Reference { get; set; }
    
    [Name("currency")]
    public string? Currency { get; set; }
    
    [Optional]
    [Name("type")]
    public CdrCallTypeEnum? Type { get; set; }

    // must be on the same order as CdrRepositoryClickHouseImpl.ColumnsName
    // This will somewhat break up the DB abstraction, but since datasets can be huge I didn't want to lose more time converting to the Entity 
    public object?[] ToObjects() => [CallerId, Recipient, CallDate, EndTime?.ToString("HH:mm:ss"), Duration, Cost, Reference, Currency, Type];
}