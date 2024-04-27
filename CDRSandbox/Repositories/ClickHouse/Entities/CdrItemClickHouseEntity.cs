using CDRSandbox.Repositories.Interfaces;

namespace CDRSandbox.Repositories.ClickHouse.Entities;

public class CdrItemClickHouseEntity : ICdrItemEntity
{
    public string CallerId { get; set; }
    public string Recipient { get; set; }
    // Although ClickHouse column is type Date, it reaches here at DateTime so let's use it
    public DateTime CallDate { get; set; }
    public string EndTime { get; set; }
    public uint Duration { get; set; }
    public float Cost { get; set; }
    public string Reference { get; set; }
    public string Currency { get; set; }
    public int? Type { get; set; } 
    
    // must be on the same order as CdrRepositoryClickHouse.ColumnsName
    public object?[] ToObjects() => [CallerId, Recipient, CallDate, EndTime, Duration, Cost, Reference, Currency, Type];
}