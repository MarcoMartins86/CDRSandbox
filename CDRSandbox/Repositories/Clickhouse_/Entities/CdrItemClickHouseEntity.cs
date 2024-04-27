using CDRSandbox.Helpers;
using CDRSandbox.Repositories.Interfaces;

namespace CDRSandbox.Repositories.Clickhouse.Entities;

public class CdrItemClickHouseEntity : ICdrItemEntity
{
    private string _callerID;
    public string CallerId
    {
        get => _callerID;
        set => _callerID = ClickHouseHelper.UnpadFixedString(value);
    }

    private string _recipient;
    public string Recipient 
    {
        get => _recipient;
        set => _recipient = ClickHouseHelper.UnpadFixedString(value);
    }

    // Although ClickHouse contains type Date, it reaches here at DateTime so let's use it
    private DateTime _callDate;
    public DateTime CallDate
    {
        get => _callDate.Date;
        set => _callDate = value;
    }
    
    public string EndTime { get; set; }
    public uint Duration { get; set; }
    public float Cost { get; set; }
    
    private string _reference;
    public string Reference 
    {
        get => _reference;
        set => _reference = ClickHouseHelper.UnpadFixedString(value);
    }
    
    public string Currency { get; set; }
    public int? Type { get; set; } 
    
    // must be on the same order as CdrRepositoryClickHouseImpl.ColumnsName
    public object?[] ToObjects() => [CallerId, Recipient, CallDate, EndTime, Duration, Cost, Reference, Currency, Type];
}