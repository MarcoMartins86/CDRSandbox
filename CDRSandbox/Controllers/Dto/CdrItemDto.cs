using CDRSandbox.Services.Models;

namespace CDRSandbox.Controllers.Dto;

public class CdrItemDto
{
    public string CallerId { get; set; }
    public string Recipient { get; set; }
    public string CallDate { get; set; }
    public string EndTime { get; set; }
    public uint Duration { get; set; }
    public float Cost { get; set; }
    public string Reference { get; set; }
    public string Currency { get; set; }
    public int? Type { get; set; }
    
    public static CdrItemDto? From(CdrItem? item)
    {
        return item != null ? new CdrItemDto()
        {
            CallerId = item.CallerId.ToString(),
            Recipient = item.Recipient.ToString(),
            CallDate = item.CallDate.ToString(),
            EndTime = item.EndTime.ToString(),
            Duration = item.Duration.TotalSeconds,
            Cost = item.Cost.Amount,
            Reference = item.Reference.ToString(),
            Currency = item.Cost.Currency.ToString(),
            Type = (int?)item.Type,
        } : null;
    }
}