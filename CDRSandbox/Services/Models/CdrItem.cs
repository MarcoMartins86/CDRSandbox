using CDRSandbox.Services.Models.ValueObjects;

namespace CDRSandbox.Services.Models;

public class CdrItem
{
    //[Name("caller_id")]
    public Phone CallerId { get; set; }
    
    //[Name("recipient")]
    public Phone Recipient { get; set; }
    
    //[Name("call_date")]
    //[Format(["dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy"])]
    public DateOnly CallDate { get; set; }
    
    //[Name("end_time")]
    public TimeOnly EndTime { get; set; }
    
    //[Name("duration")]
    public TimeSpan Duration { get; set; }
    
    //[Name("cost")]
    public Money Cost { get; set; }
    
    //[Name("reference")]
    public CdrReference Reference { get; set; }
    
    //[Optional]
    //[Name("type")]
    public CdrCallTypeEnum? Type { get; set; }

}